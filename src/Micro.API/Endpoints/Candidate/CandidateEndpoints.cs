using Micro.API.Data;
using Micro.API.Endpoints.CustomFields;
using Micro.API.Infrastructure.Auth;
using Micro.API.Infrastructure.CustomFields;
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
        var candidateDto = await db.Candidates
            .AsNoTracking()
            .Include(c => c.Applications)
                .ThenInclude(a => a.JobPosting)
            .Include(c => c.Applications)
                .ThenInclude(a => a.Feedbacks)
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.Email,
                c.Phone,
                c.CreatedAt,
                Applications = c.Applications
                    .OrderByDescending(a => a.AppliedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.JobPostingId,
                        JobTitle = a.JobPosting.Title,
                        Status = a.Status,
                        ArchivalResolution = a.ArchivalResolution,
                        a.AppliedAt,
                        Feedbacks = a.Feedbacks
                            .OrderByDescending(f => f.CreatedAt)
                            .Select(f => new CandidateFeedbackResponse(
                                f.Id,
                                f.Notes,
                                f.Score,
                                f.CreatedAt
                            )).ToList(),
                        a.RequisitionOpeningId,
                        OpeningSequenceNumber = a.RequisitionOpening != null ? a.RequisitionOpening.SequenceNumber : (int?)null,
                        RequisitionTitle = a.RequisitionOpening != null && a.RequisitionOpening.Requisition != null ? a.RequisitionOpening.Requisition.Title : null,
                        a.Interview,
                        a.Offer
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (candidateDto is null)
        {
            return Results.NotFound();
        }

        var appIds = candidateDto.Applications.Select(a => a.Id).ToList();
        var customFieldsMap = await CustomFieldPersistence.GetBatchApplicationValuesAsync(db, appIds);

        var apps = new List<CandidateApplicationResponse>();
        foreach (var a in candidateDto.Applications)
        {
            var customFields = customFieldsMap.TryGetValue(a.Id, out var cf) ? cf : new List<CustomFieldValueDto>();
            apps.Add(new CandidateApplicationResponse(
                a.Id,
                a.JobPostingId,
                a.JobTitle,
                a.Status.ToString(),
                a.ArchivalResolution.ToString(),
                a.AppliedAt,
                a.Feedbacks,
                a.RequisitionOpeningId,
                a.OpeningSequenceNumber,
                a.RequisitionTitle,
                a.Interview,
                a.Offer,
                customFields
            ));
        }

        var response = new CandidateDetailResponse(
            candidateDto.Id,
            candidateDto.FullName,
            candidateDto.Email,
            candidateDto.Phone,
            candidateDto.CreatedAt,
            apps
        );

        return Results.Ok(response);
    }
}
