using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Micro.API.Endpoints.CustomFields;
using System.Linq;
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
        group.MapPost("/{id:guid}/close", CloseJobPosting).RequireAuthorization("JobPosting:Close");
    }

    private static async Task<IResult> GetPublicJobs(MicroDbContext db)
    {
        var jobs = await db.JobPostings
            .Where(j => j.Status == JobPostingStatus.Published)
            .Select(j => new PublicJobResponse(
                j.Id,
                j.Title,
                j.Requisition.Department.Name,
                j.Description,
                j.CreatedAt
            ))
            .ToListAsync();

        return Results.Ok(jobs);
    }

    private static async Task<IResult> GetPublicJob(Guid id, MicroDbContext db)
    {
        var job = await db.JobPostings
            .Include(j => j.Requisition)
            .ThenInclude(r => r.Department)
            .Where(j => j.Id == id && j.Status == JobPostingStatus.Published)
            .FirstOrDefaultAsync();

        if (job is null) return Results.NotFound();

        // Load active, candidate-facing definitions for application global and application applied
        var candidateFacingFields = await db.CustomFieldDefinitions
            .Where(d => (d.TargetEntity == CustomFieldTargetEntity.Application_Global || d.TargetEntity == CustomFieldTargetEntity.Application_Applied)
                        && d.IsCandidateFacing && !d.IsDisabled)
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
    private static async Task<IResult> GetAdminJobs(MicroDbContext db)
    {
        var jobs = await db.JobPostings
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
            })
            .ToListAsync();
        
        return Results.Ok(jobs);
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
}
