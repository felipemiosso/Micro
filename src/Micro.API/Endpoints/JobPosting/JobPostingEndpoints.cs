using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Micro.API.Endpoints.CustomFields;
using System.Linq;
using Micro.API.Infrastructure.Pagination;
using System.Collections.Generic;

namespace Micro.API.Endpoints.JobPosting;

public static class JobPostingEndpoints
{
    public static void MapJobPostingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/jobs");

        // Public endpoints
        group.MapGet("/", GetPublicJobs).AllowAnonymous();
        group.MapGet("/{id:guid}", GetPublicJob).AllowAnonymous();

        // Management endpoints
        group.MapGet("/admin", GetAdminJobs).RequireAuthorization("JobPosting:View");
        group.MapPut("/{id:guid}", UpdateJobPosting).RequireAuthorization("JobPosting:Edit");
        group.MapPost("/{id:guid}/custom-fields", LinkCustomField).RequireAuthorization("JobPosting:Edit");
        group.MapDelete("/{id:guid}/custom-fields/{definitionId:guid}", UnlinkCustomField).RequireAuthorization("JobPosting:Edit");
        group.MapPost("/{id:guid}/close", CloseJobPosting).RequireAuthorization("JobPosting:Close");
    }

    private static async Task<IResult> GetPublicJobs(
        MicroDbContext db,
        [AsParameters] PaginationParams pagination)
    {
        if (!pagination.IsValid(out var error))
        {
            return Results.BadRequest(new { error });
        }

        var query = db.JobPostings
            .Where(j => j.Status == JobPostingStatus.Published)
            .Select(j => new PublicJobResponse(
                j.Id,
                j.Title,
                j.Requisition.Department.Name,
                j.Description,
                j.CreatedAt
            ));

        var pagedResponse = await query.ToPagedResponseAsync(pagination.GetPage(), pagination.GetPageSize());
        return Results.Ok(pagedResponse);
    }

    private static async Task<IResult> GetPublicJob(Guid id, MicroDbContext db)
    {
        var job = await db.JobPostings
            .Include(j => j.Requisition)
            .ThenInclude(r => r.Department)
            .Where(j => j.Id == id && j.Status == JobPostingStatus.Published)
            .FirstOrDefaultAsync();

        if (job is null) return Results.NotFound();

        // Load active, candidate-facing definitions (global + job-specific + requisition-specific)
        var candidateFacingFields = await db.CustomFieldDefinitions
            .Where(d => (d.TargetEntity == CustomFieldTargetEntity.Application_Global || d.TargetEntity == CustomFieldTargetEntity.Application_Applied)
                        && d.IsCandidateFacing && !d.IsDisabled
                        && (d.IsGlobal ||
                            db.JobPostingCustomFields.Any(jpcf => jpcf.JobPostingId == id && jpcf.CustomFieldDefinitionId == d.Id) ||
                            db.RequisitionCustomFields.Any(rcf => rcf.RequisitionId == job.RequisitionId && rcf.CustomFieldDefinitionId == d.Id)
                        ))
            .OrderBy(d => d.Order)
            .ToListAsync();

        var dtos = candidateFacingFields.Select(d => {
            var validationDto = string.IsNullOrEmpty(d.ValidationJson)
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<ValidationOptionsDto>(d.ValidationJson);

            return new CustomFieldDefinitionDto(
                d.Id,
                d.TargetEntity,
                d.FieldType,
                d.Label,
                d.HelpText,
                d.Order,
                d.IsRequired,
                d.IsDisabled,
                d.IsCandidateFacing,
                validationDto,
                0, // Value count not needed for candidate facing public view
                d.IsGlobal,
                d.CreatedAt,
                d.UpdatedAt
            );
        }).ToList();

        var response = new PublicJobDetailResponse(
            job.Id,
            job.Title,
            job.Requisition.Department.Name,
            job.Description,
            job.Requirements,
            job.CreatedAt,
            dtos
        );

        return Results.Ok(response);
    }

    [ResourceAction("JobPosting", "View", "List all job postings (including closed)")]
    private static async Task<IResult> GetAdminJobs(
        MicroDbContext db,
        [AsParameters] PaginationParams pagination)
    {
        if (!pagination.IsValid(out var error))
        {
            return Results.BadRequest(new { error });
        }

        var query = db.JobPostings
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new {
                j.Id,
                j.RequisitionId,
                j.Title,
                j.Description,
                j.Requirements,
                j.Status,
                j.CreatedAt,
                j.UpdatedAt,
                j.ClosedAt,
                Requisition = new {
                    Department = j.Requisition.Department.Name
                }
            });

        var pagedResponse = await query.ToPagedResponseAsync(pagination.GetPage(), pagination.GetPageSize());
        return Results.Ok(pagedResponse);
    }

    [ResourceAction("JobPosting", "Edit", "Update job posting details")]
    private static async Task<IResult> UpdateJobPosting(Guid id, UpdateJobPostingRequest request, MicroDbContext db)
    {
        var job = await db.JobPostings.FindAsync(id);
        if (job is null) return Results.NotFound();

        job.Title = request.Title;
        job.Description = request.Description;
        job.Requirements = request.Requirements;
        job.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    [ResourceAction("JobPosting", "Close", "Close a job posting")]
    private static async Task<IResult> CloseJobPosting(Guid id, MicroDbContext db)
    {
        var job = await db.JobPostings.FindAsync(id);
        if (job is null) return Results.NotFound();

        job.Status = JobPostingStatus.Closed;
        job.ClosedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    [ResourceAction("JobPosting", "Edit", "Link a selectable custom field to this job posting")]
    private static async Task<IResult> LinkCustomField(Guid id, LinkCustomFieldRequest request, MicroDbContext db)
    {
        var job = await db.JobPostings.FindAsync(id);
        if (job is null) return Results.NotFound();

        var def = await db.CustomFieldDefinitions.FindAsync(request.CustomFieldDefinitionId);
        if (def is null || def.IsGlobal || def.IsDisabled)
        {
            return Results.BadRequest("Invalid custom field definition for linkage.");
        }

        var isAllowedScope = def.TargetEntity == CustomFieldTargetEntity.JobPosting ||
                             def.TargetEntity == CustomFieldTargetEntity.Application_Global ||
                             def.TargetEntity == CustomFieldTargetEntity.Application_Applied ||
                             def.TargetEntity == CustomFieldTargetEntity.Application_Interview ||
                             def.TargetEntity == CustomFieldTargetEntity.Application_Offer;

        if (!isAllowedScope)
        {
            return Results.BadRequest("Invalid custom field target scope for Job Posting linkage.");
        }

        var exists = await db.JobPostingCustomFields.AnyAsync(jpcf => jpcf.JobPostingId == id && jpcf.CustomFieldDefinitionId == request.CustomFieldDefinitionId);
        if (!exists)
        {
            db.JobPostingCustomFields.Add(new JobPostingCustomField
            {
                JobPostingId = id,
                CustomFieldDefinitionId = request.CustomFieldDefinitionId
            });
            await db.SaveChangesAsync();
        }

        return Results.NoContent();
    }

    [ResourceAction("JobPosting", "Edit", "Unlink a selectable custom field from this job posting")]
    private static async Task<IResult> UnlinkCustomField(Guid id, Guid definitionId, MicroDbContext db)
    {
        var job = await db.JobPostings.FindAsync(id);
        if (job is null) return Results.NotFound();

        var def = await db.CustomFieldDefinitions.FindAsync(definitionId);
        if (def is null) return Results.NotFound();

        bool hasValues = false;
        if (def.TargetEntity == CustomFieldTargetEntity.JobPosting)
        {
            hasValues = await db.CustomFieldValues.AnyAsync(v => v.EntityId == id && v.CustomFieldDefinitionId == definitionId);
        }
        else
        {
            var applicationIds = await db.Applications.Where(a => a.JobPostingId == id).Select(a => a.Id).ToListAsync();
            hasValues = await db.CustomFieldValues.AnyAsync(v => v.CustomFieldDefinitionId == definitionId && applicationIds.Contains(v.EntityId));
        }

        if (hasValues)
        {
            return Results.Conflict(new { code = "FIELD_HAS_VALUES", message = "Cannot unlink custom field because it contains recorded values." });
        }

        var link = await db.JobPostingCustomFields.FirstOrDefaultAsync(jpcf => jpcf.JobPostingId == id && jpcf.CustomFieldDefinitionId == definitionId);
        if (link is not null)
        {
            db.JobPostingCustomFields.Remove(link);
            await db.SaveChangesAsync();
        }

        return Results.NoContent();
    }
}
