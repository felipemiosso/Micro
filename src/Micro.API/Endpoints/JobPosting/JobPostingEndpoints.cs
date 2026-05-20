using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

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
            .Where(j => j.Id == id && j.Status == JobPostingStatus.Published)
            .Select(j => new PublicJobDetailResponse(
                j.Id,
                j.Title,
                j.Requisition.Department.Name,
                j.Description,
                j.Requirements,
                j.CreatedAt
            ))
            .FirstOrDefaultAsync();

        return job is null ? Results.NotFound() : Results.Ok(job);
    }

    [ResourceAction("JobPosting", "View", "List all job postings (including closed)")]
    private static async Task<IResult> GetAdminJobs(MicroDbContext db)
    {
        var jobs = await db.JobPostings
            .Include(j => j.Requisition)
            .OrderByDescending(j => j.CreatedAt)
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
