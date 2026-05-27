using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.Auth;
using Micro.API.Infrastructure.CustomFields;
using Micro.API.Endpoints.CustomFields;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        group.MapPost("/{id:guid}/custom-fields", LinkCustomField).RequireAuthorization("Requisition:Edit");
        group.MapDelete("/{id:guid}/custom-fields/{definitionId:guid}", UnlinkCustomField).RequireAuthorization("Requisition:Edit");
        group.MapPut("/{id:guid}/openings/{openingId:guid}", UpdateRequisitionOpening).RequireAuthorization("Requisition:Edit");
        group.MapPost("/{id:guid}/finalize", FinalizeRequisition).RequireAuthorization("Requisition:Finalize");
        group.MapPost("/{id:guid}/close", CloseRequisition).RequireAuthorization("Requisition:Close");
    }

    [ResourceAction("Requisition", "View", "List all requisitions")]
    private static async Task<IResult> GetRequisitions(HttpContext context, MicroDbContext db)
    {
        var query = db.Requisitions
            .AsNoTracking()
            .Include(r => r.Department)
            .Include(r => r.Openings)
            .AsQueryable();

        var cfFilters = CustomFieldPersistence.ParseFilters(context.Request.Query);
        if (cfFilters.Count > 0)
        {
            var filterDefs = await db.CustomFieldDefinitions
                .Where(d => cfFilters.Keys.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id);

            foreach (var filterPair in cfFilters)
            {
                var defId = filterPair.Key;
                var filter = filterPair.Value;
                if (!filterDefs.TryGetValue(defId, out var def)) continue;

                if (def.FieldType == CustomFieldType.Number)
                {
                    if (decimal.TryParse(filter.Value, out var val))
                    {
                        query = query.Where(r => db.CustomFieldValues.Any(v =>
                            v.EntityId == r.Id &&
                            v.TargetEntity == CustomFieldTargetEntity.Requisition &&
                            v.CustomFieldDefinitionId == defId &&
                            Convert.ToDecimal(v.Value) == val
                        ));
                    }
                    if (decimal.TryParse(filter.Min, out var min))
                    {
                        query = query.Where(r => db.CustomFieldValues.Any(v =>
                            v.EntityId == r.Id &&
                            v.TargetEntity == CustomFieldTargetEntity.Requisition &&
                            v.CustomFieldDefinitionId == defId &&
                            Convert.ToDecimal(v.Value) >= min
                        ));
                    }
                    if (decimal.TryParse(filter.Max, out var max))
                    {
                        query = query.Where(r => db.CustomFieldValues.Any(v =>
                            v.EntityId == r.Id &&
                            v.TargetEntity == CustomFieldTargetEntity.Requisition &&
                            v.CustomFieldDefinitionId == defId &&
                            Convert.ToDecimal(v.Value) <= max
                        ));
                    }
                }
                else if (def.FieldType == CustomFieldType.Date)
                {
                    if (DateOnly.TryParse(filter.Value, out var val))
                    {
                        var valStr = val.ToString("yyyy-MM-dd");
                        query = query.Where(r => db.CustomFieldValues.Any(v =>
                            v.EntityId == r.Id &&
                            v.TargetEntity == CustomFieldTargetEntity.Requisition &&
                            v.CustomFieldDefinitionId == defId &&
                            v.Value == valStr
                        ));
                    }
                    if (DateOnly.TryParse(filter.Min, out var min))
                    {
                        var minStr = min.ToString("yyyy-MM-dd");
                        query = query.Where(r => db.CustomFieldValues.Any(v =>
                            v.EntityId == r.Id &&
                            v.TargetEntity == CustomFieldTargetEntity.Requisition &&
                            v.CustomFieldDefinitionId == defId &&
                            v.Value.CompareTo(minStr) >= 0
                        ));
                    }
                    if (DateOnly.TryParse(filter.Max, out var max))
                    {
                        var maxStr = max.ToString("yyyy-MM-dd");
                        query = query.Where(r => db.CustomFieldValues.Any(v =>
                            v.EntityId == r.Id &&
                            v.TargetEntity == CustomFieldTargetEntity.Requisition &&
                            v.CustomFieldDefinitionId == defId &&
                            v.Value.CompareTo(maxStr) <= 0
                        ));
                    }
                }
                else if (def.FieldType == CustomFieldType.Boolean)
                {
                    if (!string.IsNullOrEmpty(filter.Value))
                    {
                        var valStr = filter.Value.ToLower();
                        query = query.Where(r => db.CustomFieldValues.Any(v =>
                            v.EntityId == r.Id &&
                            v.TargetEntity == CustomFieldTargetEntity.Requisition &&
                            v.CustomFieldDefinitionId == defId &&
                            v.Value == valStr
                        ));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(filter.Value))
                    {
                        query = query.Where(r => db.CustomFieldValues.Any(v =>
                            v.EntityId == r.Id &&
                            v.TargetEntity == CustomFieldTargetEntity.Requisition &&
                            v.CustomFieldDefinitionId == defId &&
                            EF.Functions.ILike(v.Value, $"%{filter.Value}%")
                        ));
                    }
                }
            }
        }

        var requisitions = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var reqIds = requisitions.Select(r => r.Id).ToList();
        var customFieldsMap = await CustomFieldPersistence.GetBatchRequisitionValuesAsync(db, reqIds);

        var results = new List<object>();
        foreach (var r in requisitions)
        {
            var customFields = customFieldsMap.TryGetValue(r.Id, out var cf) ? cf : new List<CustomFieldValueDto>();
            results.Add(new {
                r.Id,
                r.Title,
                r.DepartmentId,
                Department = new { r.Department.Id, r.Department.Name, r.Department.IsActive },
                r.SalaryBandId,
                r.CostCenterId,
                r.OpeningsCount,
                r.Status,
                r.CreatedBy,
                r.HiringManagerId,
                r.RecruiterId,
                r.EmploymentType,
                r.WorkplaceType,
                r.Location,
                r.JobDescription,
                r.IsInternalOnly,
                r.CreatedAt,
                r.FinalizedAt,
                r.PublishedAt,
                r.OfferAcceptedAt,
                r.CandidateStartedAt,
                r.ClosedAt,
                r.TargetStartDate,
                Openings = r.Openings.Select(o => new { o.Id, o.SequenceNumber, o.TargetStartDate, o.Status, o.CandidateId }),
                CustomFields = customFields
            });
        }
        return Results.Ok(results);
    }

    [ResourceAction("Requisition", "View", "View requisition details")]
    private static async Task<IResult> GetRequisition(Guid id, MicroDbContext db)
    {
        var r = await db.Requisitions
            .Include(r => r.Department)
            .Include(r => r.SalaryBand)
            .Include(r => r.CostCenter)
            .Include(r => r.Openings).ThenInclude(o => o.Candidate)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (r is null) return Results.NotFound();

        var customFields = await CustomFieldPersistence.GetValuesAsync(db, CustomFieldTargetEntity.Requisition, r.Id);
        return Results.Ok(new {
            r.Id,
            r.Title,
            r.DepartmentId,
            Department = new { r.Department.Id, r.Department.Name, r.Department.IsActive },
            r.SalaryBandId,
            SalaryBand = new { r.SalaryBand.Id, r.SalaryBand.Name, MinSalary = r.SalaryBand.MinAmount, MaxSalary = r.SalaryBand.MaxAmount },
            r.CostCenterId,
            CostCenter = new { r.CostCenter.Id, r.CostCenter.Code, r.CostCenter.Name },
            r.OpeningsCount,
            r.Status,
            r.CreatedBy,
            r.HiringManagerId,
            r.RecruiterId,
            r.EmploymentType,
            r.WorkplaceType,
            r.Location,
            r.JobDescription,
            r.IsInternalOnly,
            r.CreatedAt,
            r.FinalizedAt,
            r.PublishedAt,
            r.OfferAcceptedAt,
            r.CandidateStartedAt,
            r.ClosedAt,
            r.TargetStartDate,
            Openings = r.Openings.Select(o => new { o.Id, o.SequenceNumber, o.TargetStartDate, o.Status, o.CandidateId, Candidate = o.Candidate != null ? new { o.Candidate.Id, o.Candidate.FullName, o.Candidate.Email } : null }),
            CustomFields = customFields
        });
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

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            db.Requisitions.Add(requisition);
            await db.SaveChangesAsync();

            if (request.LinkedCustomFieldIds is not null)
            {
                foreach (var defId in request.LinkedCustomFieldIds)
                {
                    db.RequisitionCustomFields.Add(new RequisitionCustomField
                    {
                        RequisitionId = requisition.Id,
                        CustomFieldDefinitionId = defId
                    });
                }
                await db.SaveChangesAsync();
            }

            var cfErrors = await CustomFieldPersistence.ValidateAsync(
                db, CustomFieldTargetEntity.Requisition, requisition.Id,
                request.CustomFieldValues ?? [], CancellationToken.None);

            if (cfErrors is not null)
            {
                await transaction.RollbackAsync();
                return Results.ValidationProblem(cfErrors);
            }

            await CustomFieldPersistence.PersistAsync(
                db, CustomFieldTargetEntity.Requisition, requisition.Id,
                request.CustomFieldValues ?? [], CancellationToken.None);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }

        var customFields = await CustomFieldPersistence.GetValuesAsync(db, CustomFieldTargetEntity.Requisition, requisition.Id);
        return Results.Created($"/api/requisitions/{requisition.Id}", new {
            requisition.Id,
            requisition.Title,
            requisition.DepartmentId,
            requisition.SalaryBandId,
            requisition.CostCenterId,
            requisition.OpeningsCount,
            requisition.EmploymentType,
            requisition.WorkplaceType,
            requisition.Location,
            requisition.JobDescription,
            requisition.IsInternalOnly,
            requisition.TargetStartDate,
            requisition.Status,
            requisition.CreatedBy,
            requisition.CreatedAt,
            Openings = requisition.Openings.Select(o => new { o.Id, o.SequenceNumber, o.TargetStartDate, o.Status }),
            CustomFields = customFields
        });
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

        var cfErrors = await CustomFieldPersistence.ValidateAsync(
            db, CustomFieldTargetEntity.Requisition, requisition.Id,
            request.CustomFieldValues ?? [], CancellationToken.None);

        if (cfErrors is not null)
            return Results.ValidationProblem(cfErrors);

        await db.SaveChangesAsync();

        await CustomFieldPersistence.PersistAsync(
            db, CustomFieldTargetEntity.Requisition, requisition.Id,
            request.CustomFieldValues ?? [], CancellationToken.None);
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

    [ResourceAction("Requisition", "Edit", "Link a selectable custom field to this requisition")]
    private static async Task<IResult> LinkCustomField(Guid id, LinkCustomFieldRequest request, MicroDbContext db)
    {
        var requisition = await db.Requisitions.FindAsync(id);
        if (requisition is null) return Results.NotFound();

        var def = await db.CustomFieldDefinitions.FindAsync(request.CustomFieldDefinitionId);
        if (def is null || def.IsGlobal || def.IsDisabled || def.TargetEntity != CustomFieldTargetEntity.Requisition)
        {
            return Results.BadRequest("Invalid custom field definition for linkage.");
        }

        var exists = await db.RequisitionCustomFields.AnyAsync(rcf => rcf.RequisitionId == id && rcf.CustomFieldDefinitionId == request.CustomFieldDefinitionId);
        if (!exists)
        {
            db.RequisitionCustomFields.Add(new RequisitionCustomField
            {
                RequisitionId = id,
                CustomFieldDefinitionId = request.CustomFieldDefinitionId
            });
            await db.SaveChangesAsync();
        }

        return Results.NoContent();
    }

    [ResourceAction("Requisition", "Edit", "Unlink a selectable custom field from this requisition")]
    private static async Task<IResult> UnlinkCustomField(Guid id, Guid definitionId, MicroDbContext db)
    {
        var requisition = await db.Requisitions.FindAsync(id);
        if (requisition is null) return Results.NotFound();

        var hasValues = await db.CustomFieldValues.AnyAsync(v => v.EntityId == id && v.CustomFieldDefinitionId == definitionId);
        if (hasValues)
        {
            return Results.Conflict(new { code = "FIELD_HAS_VALUES", message = "Cannot unlink custom field because it contains recorded values." });
        }

        var link = await db.RequisitionCustomFields.FirstOrDefaultAsync(rcf => rcf.RequisitionId == id && rcf.CustomFieldDefinitionId == definitionId);
        if (link is not null)
        {
            db.RequisitionCustomFields.Remove(link);
            await db.SaveChangesAsync();
        }

        return Results.NoContent();
    }
}
