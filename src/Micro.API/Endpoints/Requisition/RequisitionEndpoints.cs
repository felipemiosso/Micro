using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints.Requisition;

public static class RequisitionEndpoints
{
    public static void MapRequisitionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/requisitions");

        group.MapGet("/", GetRequisitions).RequireAuthorization("Requisition:View");
        group.MapGet("/{id:guid}", GetRequisition).RequireAuthorization("Requisition:View");
        group.MapPost("/", CreateRequisition).RequireAuthorization("Requisition:Create");
        group.MapPut("/{id:guid}", UpdateRequisition).RequireAuthorization("Requisition:Edit");
        group.MapPut("/{id:guid}/openings/{openingId:guid}", UpdateRequisitionOpening).RequireAuthorization("Requisition:Edit");
        group.MapPost("/{id:guid}/finalize", FinalizeRequisition).RequireAuthorization("Requisition:Finalize");
        group.MapPost("/{id:guid}/close", CloseRequisition).RequireAuthorization("Requisition:Close");
    }

    [ResourceAction("Requisition", "View", "List all requisitions")]
    private static async Task<IResult> GetRequisitions(MicroDbContext db)
    {
        var requisitions = await db.Requisitions
            .Include(r => r.Department)
            .Include(r => r.Openings)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return Results.Ok(requisitions);
    }

    [ResourceAction("Requisition", "View", "View requisition details")]
    private static async Task<IResult> GetRequisition(Guid id, MicroDbContext db)
    {
        var requisition = await db.Requisitions
            .Include(r => r.Department)
            .Include(r => r.SalaryBand)
            .Include(r => r.CostCenter)
            .Include(r => r.Openings).ThenInclude(o => o.Candidate)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        return requisition is null ? Results.NotFound() : Results.Ok(requisition);
    }

    [ResourceAction("Requisition", "Create", "Create a new requisition")]
    private static async Task<IResult> CreateRequisition(CreateRequisitionRequest request, MicroDbContext db, AuthUser authUser)
    {
        var requisition = new Data.Models.Requisition
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            DepartmentId = request.DepartmentId,
            SalaryBandId = request.SalaryBandId,
            CostCenterId = request.CostCenterId,
            OpeningsCount = request.OpeningsCount,
            EmploymentType = request.EmploymentType,
            WorkplaceType = request.WorkplaceType,
            Location = request.Location,
            JobDescription = request.JobDescription,
            IsInternalOnly = request.IsInternalOnly,
            TargetStartDate = request.TargetStartDate,
            Status = RequisitionStatus.Draft,
            CreatedBy = authUser.Name ?? authUser.Email,
            CreatedAt = DateTime.UtcNow
        };

        // Initialize Openings in Draft State
        for (int i = 1; i <= request.OpeningsCount; i++)
        {
            var customOpening = request.Openings?.FirstOrDefault(o => o.SequenceNumber == i);
            requisition.Openings.Add(new RequisitionOpening
            {
                Id = Guid.NewGuid(),
                SequenceNumber = i,
                TargetStartDate = customOpening?.TargetStartDate ?? request.TargetStartDate,
                Status = OpeningStatus.Open
            });
        }

        db.Requisitions.Add(requisition);
        await db.SaveChangesAsync();

        return Results.Created($"/api/requisitions/{requisition.Id}", requisition);
    }

    [ResourceAction("Requisition", "Edit", "Update draft requisition")]
    private static async Task<IResult> UpdateRequisition(Guid id, UpdateRequisitionRequest request, MicroDbContext db)
    {
        var requisition = await db.Requisitions
            .Include(r => r.Openings)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (requisition is null) return Results.NotFound();
        if (requisition.Status != RequisitionStatus.Draft) return Results.BadRequest("Only draft requisitions can be updated.");

        requisition.Title = request.Title;
        requisition.DepartmentId = request.DepartmentId;
        requisition.SalaryBandId = request.SalaryBandId;
        requisition.CostCenterId = request.CostCenterId;
        requisition.OpeningsCount = request.OpeningsCount;
        requisition.EmploymentType = request.EmploymentType;
        requisition.WorkplaceType = request.WorkplaceType;
        requisition.Location = request.Location;
        requisition.JobDescription = request.JobDescription;
        requisition.IsInternalOnly = request.IsInternalOnly;
        requisition.TargetStartDate = request.TargetStartDate;

        // Synchronize Openings
        // 1. Remove excess openings
        var existingOpenings = requisition.Openings.ToList();
        foreach (var opening in existingOpenings.Where(o => o.SequenceNumber > request.OpeningsCount))
        {
            db.RequisitionOpenings.Remove(opening);
            requisition.Openings.Remove(opening);
        }

        // 2. Add or update remaining openings
        for (int i = 1; i <= request.OpeningsCount; i++)
        {
            var customOpening = request.Openings?.FirstOrDefault(o => o.SequenceNumber == i);
            var targetDate = customOpening?.TargetStartDate ?? request.TargetStartDate;

            var existing = requisition.Openings.FirstOrDefault(o => o.SequenceNumber == i);
            if (existing != null)
            {
                existing.TargetStartDate = targetDate;
            }
            else
            {
                requisition.Openings.Add(new RequisitionOpening
                {
                    Id = Guid.NewGuid(),
                    SequenceNumber = i,
                    TargetStartDate = targetDate,
                    Status = OpeningStatus.Open
                });
            }
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    public record UpdateRequisitionOpeningRequest(DateTime? TargetStartDate, OpeningStatus Status);

    [ResourceAction("Requisition", "Edit", "Update an individual requisition opening")]
    private static async Task<IResult> UpdateRequisitionOpening(
        Guid id,
        Guid openingId,
        UpdateRequisitionOpeningRequest request,
        MicroDbContext db)
    {
        var opening = await db.RequisitionOpenings
            .Include(o => o.Requisition)
            .FirstOrDefaultAsync(o => o.Id == openingId && o.RequisitionId == id);

        if (opening is null) return Results.NotFound();

        if (opening.Status == OpeningStatus.Filled)
        {
            return Results.BadRequest("Cannot modify a filled opening.");
        }

        if (request.Status == OpeningStatus.Filled)
        {
            return Results.BadRequest("Cannot manually set status to Filled. Assign a candidate to fill.");
        }

        opening.TargetStartDate = request.TargetStartDate;
        opening.Status = request.Status;

        // If status becomes Cancelled, check if all openings are now Filled or Cancelled
        if (request.Status == OpeningStatus.Cancelled)
        {
            var otherOpenings = await db.RequisitionOpenings
                .Where(o => o.RequisitionId == id && o.Id != openingId)
                .ToListAsync();

            if (otherOpenings.All(o => o.Status == OpeningStatus.Filled || o.Status == OpeningStatus.Cancelled))
            {
                var requisition = opening.Requisition;
                requisition.Status = RequisitionStatus.Closed;
                requisition.ClosedAt = DateTime.UtcNow;

                var posting = await db.JobPostings.FirstOrDefaultAsync(p => p.RequisitionId == id);
                if (posting != null && posting.Status == JobPostingStatus.Published)
                {
                    posting.Status = JobPostingStatus.Closed;
                    posting.ClosedAt = DateTime.UtcNow;
                }
            }
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    [ResourceAction("Requisition", "Finalize", "Finalize requisition and publish job")]
    private static async Task<IResult> FinalizeRequisition(Guid id, MicroDbContext db)
    {
        var requisition = await db.Requisitions
            .Include(r => r.Openings)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (requisition is null) return Results.NotFound();
        if (requisition.Status != RequisitionStatus.Draft) return Results.BadRequest("Only draft requisitions can be finalized.");

        requisition.Status = RequisitionStatus.Finalized;
        requisition.FinalizedAt = DateTime.UtcNow;

        // Ensure openings status is Open
        foreach (var opening in requisition.Openings)
        {
            opening.Status = OpeningStatus.Open;
        }

        // Automatically create a Job Posting
        var jobPosting = new Micro.API.Data.Models.JobPosting
        {
            RequisitionId = requisition.Id,
            Title = requisition.Title,
            Description = requisition.JobDescription,
            Requirements = "Requirements from requisition.",
            Status = JobPostingStatus.Published,
            CreatedAt = DateTime.UtcNow
        };

        db.JobPostings.Add(jobPosting);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    [ResourceAction("Requisition", "Close", "Close requisition and job posting")]
    private static async Task<IResult> CloseRequisition(Guid id, MicroDbContext db)
    {
        var requisition = await db.Requisitions.FindAsync(id);
        if (requisition is null) return Results.NotFound();

        requisition.Status = RequisitionStatus.Closed;
        requisition.ClosedAt = DateTime.UtcNow;

        var posting = await db.JobPostings.FirstOrDefaultAsync(p => p.RequisitionId == id);
        if (posting != null && posting.Status == JobPostingStatus.Published)
        {
            posting.Status = JobPostingStatus.Closed;
            posting.ClosedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
