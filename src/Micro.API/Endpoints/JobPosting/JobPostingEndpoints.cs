using Micro.API.Data;
using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints.JobPosting;

public static class JobPostingEndpoints
{
    public static void MapJobPostingEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/jobs").AllowAnonymous();
        publicGroup.MapGet("/", GetPublicJobs);
        publicGroup.MapGet("/{id:guid}", GetPublicJob);

        var adminGroup = app.MapGroup("/api/admin/jobs");
        adminGroup.MapGet("/", GetAdminJobs);
        adminGroup.MapPut("/{id:guid}", UpdateJobPosting);
        adminGroup.MapPost("/{id:guid}/close", CloseJobPosting);
    }

    private static async Task<IResult> GetPublicJobs(MicroDbContext db)
    {
        var jobs = await db.JobPostings
            .Where(j => j.Status == JobPostingStatus.Published)
            .Select(j => new PublicJobResponse(
                j.Id,
                j.Title,
                j.Requisition.Department,
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
                j.Requisition.Department,
                j.Description,
                j.Requirements,
                j.CreatedAt
            ))
            .FirstOrDefaultAsync();

        return job is null ? Results.NotFound() : Results.Ok(job);
    }

    private static async Task<IResult> GetAdminJobs(MicroDbContext db)
    {
        var jobs = await db.JobPostings
            .Include(j => j.Requisition)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
        
        return Results.Ok(jobs);
    }

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
