using Micro.API.Data;
using Micro.API.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints.Candidate;

public static class CandidateEndpoints
{
    public static void MapCandidateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/candidates");

        group.MapGet("/{id:guid}", GetCandidateDetail).RequireAuthorization("Candidate:View");
    }

    [ResourceAction("Candidate", "View", "View candidate profile and application history")]
    private static async Task<IResult> GetCandidateDetail(Guid id, MicroDbContext db)
    {
        var candidate = await db.Candidates
            .Include(c => c.Applications)
                .ThenInclude(a => a.JobPosting)
            .Include(c => c.Applications)
                .ThenInclude(a => a.Feedbacks)
            .Where(c => c.Id == id)
            .Select(c => new CandidateDetailResponse(
                c.Id,
                c.FullName,
                c.Email,
                c.Phone,
                c.CreatedAt,
                c.Applications
                    .OrderByDescending(a => a.AppliedAt)
                    .Select(a => new CandidateApplicationResponse(
                        a.Id,
                        a.JobPostingId,
                        a.JobPosting.Title,
                        (int)a.Status,
                        (int)a.ArchivalResolution,
                        a.AppliedAt,
                        a.Feedbacks
                            .OrderByDescending(f => f.CreatedAt)
                            .Select(f => new CandidateFeedbackResponse(
                                f.Id,
                                f.Notes,
                                f.Score,
                                f.CreatedAt
                            )).ToList()
                    )).ToList()
            ))
            .FirstOrDefaultAsync();

        return candidate is null ? Results.NotFound() : Results.Ok(candidate);
    }
}
