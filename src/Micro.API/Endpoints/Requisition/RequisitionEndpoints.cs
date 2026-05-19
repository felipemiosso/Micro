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
        group.MapPost("/{id:guid}/finalize", FinalizeRequisition).RequireAuthorization("Requisition:Finalize");
        group.MapPost("/{id:guid}/close", CloseRequisition).RequireAuthorization("Requisition:Close");
    }

    [ResourceAction("Requisition", "View", "List all requisitions")]
    private static async Task<IResult> GetRequisitions(MicroDbContext db)
    {
        var requisitions = await db.Requisitions
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return Results.Ok(requisitions);
    }

    [ResourceAction("Requisition", "View", "View requisition details")]
    private static async Task<IResult> GetRequisition(Guid id, MicroDbContext db)
    {
        var requisition = await db.Requisitions.FindAsync(id);
        return requisition is null ? Results.NotFound() : Results.Ok(requisition);
    }

    [ResourceAction("Requisition", "Create", "Create a new requisition")]
    private static async Task<IResult> CreateRequisition(CreateRequisitionRequest request, MicroDbContext db, AuthUser authUser)
    {
        var requisition = new Data.Models.Requisition
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Department = request.Department,
            Openings = request.Openings,
            Status = RequisitionStatus.Draft,
            CreatedBy = authUser.Name ?? authUser.Email,
            CreatedAt = DateTime.UtcNow
        };

        db.Requisitions.Add(requisition);
        await db.SaveChangesAsync();

        return Results.Created($"/api/requisitions/{requisition.Id}", requisition);
    }

    [ResourceAction("Requisition", "Edit", "Update draft requisition")]
    private static async Task<IResult> UpdateRequisition(Guid id, UpdateRequisitionRequest request, MicroDbContext db)
    {
        var requisition = await db.Requisitions.FindAsync(id);
        if (requisition is null) return Results.NotFound();
        if (requisition.Status != RequisitionStatus.Draft) return Results.BadRequest("Only draft requisitions can be updated.");

        requisition.Title = request.Title;
        requisition.Department = request.Department;
        requisition.Openings = request.Openings;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    [ResourceAction("Requisition", "Finalize", "Finalize requisition and publish job")]
    private static async Task<IResult> FinalizeRequisition(Guid id, MicroDbContext db)
    {
        var requisition = await db.Requisitions.FindAsync(id);
        if (requisition is null) return Results.NotFound();
        if (requisition.Status != RequisitionStatus.Draft) return Results.BadRequest("Only draft requisitions can be finalized.");

        requisition.Status = RequisitionStatus.Finalized;
        requisition.FinalizedAt = DateTime.UtcNow;

        // Automatically create a Job Posting
        var jobPosting = new Data.Models.JobPosting
        {
            Id = Guid.NewGuid(),
            RequisitionId = requisition.Id,
            Title = requisition.Title,
            Description = $"Open position for {requisition.Title} in {requisition.Department} department.",
            Requirements = "Requirements to be defined.",
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
