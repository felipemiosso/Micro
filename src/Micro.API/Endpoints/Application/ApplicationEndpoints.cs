using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.Auth;
using Micro.API.Infrastructure.CustomFields;
using Micro.API.Endpoints.CustomFields;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints.Application;

public static class ApplicationEndpoints
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        // Public endpoint
        app.MapPost("/api/public/jobs/{jobPostingId:guid}/apply", ApplyToJob)
            .AllowAnonymous()
            .DisableAntiforgery();

        app.MapGet("/api/job-postings/{jobPostingId:guid}/available-openings", GetAvailableOpenings)
            .RequireAuthorization("Application:View");

        // Management endpoints
        var group = app.MapGroup("/api/applications");
        
        group.MapGet("/", GetAdminApplications).RequireAuthorization("Application:View");
        group.MapGet("/{id:guid}", GetApplicationDetail).RequireAuthorization("Application:View");
        group.MapGet("/{id:guid}/resume", GetApplicationResume).RequireAuthorization("Application:ViewResume");
        group.MapPut("/{id:guid}/status", UpdateApplicationStatus).RequireAuthorization("Application:Edit");
        group.MapPut("/{id:guid}/interview-details", UpdateInterviewDetails).RequireAuthorization("Application:Edit");
        group.MapPut("/{id:guid}/offer-details", UpdateOfferDetails).RequireAuthorization("Application:Edit");
        group.MapPost("/{id:guid}/feedback", AddFeedback).RequireAuthorization("Application:Feedback");
    }

    private static async Task<IResult> ApplyToJob(
        Guid jobPostingId,
        HttpContext context,
        MicroDbContext db,
        IWebHostEnvironment env)
    {
        var job = await db.JobPostings.FindAsync(jobPostingId);
        if (job is null || job.Status != JobPostingStatus.Published)
        {
            return Results.NotFound("Job posting not found or is no longer accepting applications.");
        }

        if (!context.Request.HasFormContentType)
        {
            return Results.BadRequest("Invalid content type. Expected multipart/form-data.");
        }

        var form = await context.Request.ReadFormAsync();
        var name = form["name"].ToString();
        var email = form["email"].ToString();
        var phone = form["phone"].ToString();
        var resumeFile = form.Files.GetFile("resume");

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || resumeFile is null)
        {
            return Results.BadRequest("Name, email, and resume are required.");
        }

        // Parse custom fields from form
        var customFieldInputs = new List<CustomFieldValueInput>();
        foreach (var key in form.Keys)
        {
            if (key.StartsWith("customFields[") || key.StartsWith("customFieldValues["))
            {
                var prefixLength = key.StartsWith("customFields[") ? "customFields[".Length : "customFieldValues[".Length;
                var rest = key[prefixLength..];
                if (rest.EndsWith("]") && Guid.TryParse(rest[..^1], out var defId))
                {
                    var val = form[key].ToString();
                    customFieldInputs.Add(new CustomFieldValueInput(defId, string.IsNullOrWhiteSpace(val) ? null : val));
                }
            }
        }

        // Validate custom fields
        var cfErrors = await CustomFieldPersistence.ValidateCandidateFacingAsync(
            db, jobPostingId, Guid.Empty, customFieldInputs, CancellationToken.None);
        if (cfErrors is not null)
        {
            return Results.ValidationProblem(cfErrors);
        }

        if (resumeFile.Length > 5 * 1024 * 1024)
        {
            return Results.BadRequest("File too large. Max 5MB allowed.");
        }

        if (Path.GetExtension(resumeFile.FileName).ToLower() != ".pdf")
        {
            return Results.BadRequest("Only PDF files are allowed.");
        }

        // Candidate check/creation
        var candidate = await db.Candidates.FirstOrDefaultAsync(c => c.Email == email);
        if (candidate is null)
        {
            candidate = new Data.Models.Candidate
            {
                Id = Guid.NewGuid(),
                FullName = name,
                Email = email,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                CreatedAt = DateTime.UtcNow
            };
            db.Candidates.Add(candidate);
        }

        // Duplicate application check
        var exists = await db.Applications.AnyAsync(a => a.JobPostingId == jobPostingId && a.CandidateId == candidate.Id);
        if (exists)
        {
            return Results.Conflict("You have already applied for this position.");
        }

        // Save file
        var uploadsPath = Path.Combine(env.ContentRootPath, "Storage", "Resumes");
        if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}.pdf";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await resumeFile.CopyToAsync(stream);
        }

        var application = new Micro.API.Data.Models.Application
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPostingId,
            CandidateId = candidate.Id,
            ResumePath = filePath,
            AppliedAt = DateTime.UtcNow,
            Status = ApplicationStatus.Applied
        };

        db.Applications.Add(application);
        await db.SaveChangesAsync();

        // Persist candidate facing custom fields
        if (customFieldInputs.Count > 0)
        {
            await CustomFieldPersistence.PersistCandidateFacingAsync(db, jobPostingId, application.Id, customFieldInputs, CancellationToken.None);
            await db.SaveChangesAsync();
        }

        return Results.Created($"/api/applications/{application.Id}", new { application.Id });
    }

    [ResourceAction("Application", "View", "List and filter all applications")]
    private static async Task<IResult> GetAdminApplications(
        HttpContext context,
        Guid? jobPostingId,
        string? search,
        MicroDbContext db)
    {
        var query = db.Applications
            .Include(a => a.JobPosting)
            .Include(a => a.Candidate)
            .AsQueryable();
        
        if (jobPostingId.HasValue)
        {
            query = query.Where(a => a.JobPostingId == jobPostingId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(a => a.Candidate.FullName.ToLower().Contains(s) || a.Candidate.Email.ToLower().Contains(s));
        }

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
                        query = query.Where(a => db.CustomFieldValues.Any(v =>
                            v.EntityId == a.Id &&
                            v.CustomFieldDefinitionId == defId &&
                            Convert.ToDecimal(v.Value) == val
                        ));
                    }
                    if (decimal.TryParse(filter.Min, out var min))
                    {
                        query = query.Where(a => db.CustomFieldValues.Any(v =>
                            v.EntityId == a.Id &&
                            v.CustomFieldDefinitionId == defId &&
                            Convert.ToDecimal(v.Value) >= min
                        ));
                    }
                    if (decimal.TryParse(filter.Max, out var max))
                    {
                        query = query.Where(a => db.CustomFieldValues.Any(v =>
                            v.EntityId == a.Id &&
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
                        query = query.Where(a => db.CustomFieldValues.Any(v =>
                            v.EntityId == a.Id &&
                            v.CustomFieldDefinitionId == defId &&
                            v.Value == valStr
                        ));
                    }
                    if (DateOnly.TryParse(filter.Min, out var min))
                    {
                        var minStr = min.ToString("yyyy-MM-dd");
                        query = query.Where(a => db.CustomFieldValues.Any(v =>
                            v.EntityId == a.Id &&
                            v.CustomFieldDefinitionId == defId &&
                            v.Value.CompareTo(minStr) >= 0
                        ));
                    }
                    if (DateOnly.TryParse(filter.Max, out var max))
                    {
                        var maxStr = max.ToString("yyyy-MM-dd");
                        query = query.Where(a => db.CustomFieldValues.Any(v =>
                            v.EntityId == a.Id &&
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
                        query = query.Where(a => db.CustomFieldValues.Any(v =>
                            v.EntityId == a.Id &&
                            v.CustomFieldDefinitionId == defId &&
                            v.Value == valStr
                        ));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(filter.Value))
                    {
                        query = query.Where(a => db.CustomFieldValues.Any(v =>
                            v.EntityId == a.Id &&
                            v.CustomFieldDefinitionId == defId &&
                            EF.Functions.ILike(v.Value, $"%{filter.Value}%")
                        ));
                    }
                }
            }
        }

        var apps = await query
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        var results = new List<object>();
        foreach (var a in apps)
        {
            var customFields = await CustomFieldPersistence.GetApplicationValuesAsync(db, a.Id, a.Status);
            results.Add(new {
                a.Id,
                a.JobPostingId,
                a.CandidateId,
                JobTitle = a.JobPosting.Title,
                CandidateName = a.Candidate.FullName,
                CandidateEmail = a.Candidate.Email,
                CandidatePhone = a.Candidate.Phone,
                a.Status,
                a.ArchivalResolution,
                a.AppliedAt,
                InterviewDetails = a.Interview,
                OfferDetails = a.Offer,
                CustomFields = customFields
            });
        }

        return Results.Ok(results);
    }

    [ResourceAction("Application", "ViewResume", "Download candidate resume")]
    private static async Task<IResult> GetApplicationResume(Guid id, MicroDbContext db)
    {
        var application = await db.Applications.Include(a => a.Candidate).FirstOrDefaultAsync(a => a.Id == id);
        if (application is null) return Results.NotFound();

        if (!File.Exists(application.ResumePath))
        {
            return Results.NotFound("Resume file not found on server.");
        }

        var bytes = await File.ReadAllBytesAsync(application.ResumePath);
        return Results.File(bytes, "application/pdf", $"{application.Candidate.FullName}_Resume.pdf");
    }

    [ResourceAction("Application", "Details", "View application details and feedback")]
    private static async Task<IResult> GetApplicationDetail(Guid id, MicroDbContext db)
    {
        var application = await db.Applications
            .Include(a => a.JobPosting)
            .Include(a => a.Candidate)
            .Include(a => a.Feedbacks.OrderByDescending(f => f.CreatedAt))
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null) return Results.NotFound();

        var customFields = await CustomFieldPersistence.GetApplicationValuesAsync(db, application.Id, application.Status);

        return Results.Ok(new {
            application.Id,
            application.JobPostingId,
            JobTitle = application.JobPosting.Title,
            CandidateName = application.Candidate.FullName,
            CandidateEmail = application.Candidate.Email,
            CandidatePhone = application.Candidate.Phone,
            application.Status,
            application.ArchivalResolution,
            application.AppliedAt,
            InterviewDetails = application.Interview,
            OfferDetails = application.Offer,
            Feedbacks = application.Feedbacks.Select(f => new {
                f.Id,
                f.Notes,
                f.Score,
                f.CreatedAt
            }),
            CustomFields = customFields
        });
    }

    [ResourceAction("Application", "Edit", "Update application stage or archive")]
    private static async Task<IResult> UpdateApplicationStatus(
        Guid id,
        UpdateStatusRequest request,
        MicroDbContext db)
    {
        var application = await db.Applications
            .Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (application is null) return Results.NotFound();

        if (request.CustomFieldValues is not null)
        {
            var cfErrors = await CustomFieldPersistence.ValidateApplicationAsync(
                db, application.Id, request.Status, request.CustomFieldValues, CancellationToken.None);
            if (cfErrors is not null)
                return Results.ValidationProblem(cfErrors);
        }

        if (request.Status == ApplicationStatus.Archive && request.Resolution == ArchivalResolution.None)
        {
            return Results.BadRequest("Archival resolution is required when moving to Archive status.");
        }

        if (request.Status == ApplicationStatus.Archive && request.Resolution == ArchivalResolution.Hired)
        {
            if (!request.RequisitionOpeningId.HasValue)
            {
                return Results.BadRequest("RequisitionOpeningId is required when status is Hired.");
            }

            var opening = await db.RequisitionOpenings
                .Include(o => o.Requisition)
                .FirstOrDefaultAsync(o => o.Id == request.RequisitionOpeningId.Value && o.RequisitionId == application.JobPosting.RequisitionId);

            if (opening is null)
            {
                return Results.BadRequest("Requisition opening not found or does not belong to the correct requisition.");
            }

            if (opening.Status != OpeningStatus.Open)
            {
                return Results.BadRequest("Selected opening is not available.");
            }

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                application.Status = request.Status;
                application.ArchivalResolution = request.Resolution;
                application.RequisitionOpeningId = request.RequisitionOpeningId;
                application.UpdatedAt = DateTime.UtcNow;

                opening.Status = OpeningStatus.Filled;
                opening.CandidateId = application.CandidateId;

                // Check if all openings of this requisition are now Filled or Cancelled
                var otherOpenings = await db.RequisitionOpenings
                    .Where(o => o.RequisitionId == opening.RequisitionId && o.Id != opening.Id)
                    .ToListAsync();

                if (otherOpenings.All(o => o.Status == OpeningStatus.Filled || o.Status == OpeningStatus.Cancelled))
                {
                    var requisition = opening.Requisition;
                    requisition.Status = RequisitionStatus.Closed;
                    requisition.ClosedAt = DateTime.UtcNow;

                    var posting = await db.JobPostings.FirstOrDefaultAsync(p => p.RequisitionId == requisition.Id);
                    if (posting != null && posting.Status == JobPostingStatus.Published)
                    {
                        posting.Status = JobPostingStatus.Closed;
                        posting.ClosedAt = DateTime.UtcNow;
                    }
                }

                await db.SaveChangesAsync();

                if (request.CustomFieldValues is not null)
                {
                    await CustomFieldPersistence.PersistApplicationAsync(
                        db, application.Id, request.Status, request.CustomFieldValues, CancellationToken.None);
                    await db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Results.NoContent();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        application.Status = request.Status;
        application.ArchivalResolution = request.Resolution;
        application.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        if (request.CustomFieldValues is not null)
        {
            await CustomFieldPersistence.PersistApplicationAsync(
                db, application.Id, request.Status, request.CustomFieldValues, CancellationToken.None);
            await db.SaveChangesAsync();
        }

        return Results.NoContent();
    }

    [ResourceAction("Application", "View", "Get available (open) openings for a job posting's requisition")]
    private static async Task<IResult> GetAvailableOpenings(Guid jobPostingId, MicroDbContext db)
    {
        var jobPosting = await db.JobPostings.FindAsync(jobPostingId);
        if (jobPosting is null) return Results.NotFound();

        var openings = await db.RequisitionOpenings
            .Where(o => o.RequisitionId == jobPosting.RequisitionId && o.Status == OpeningStatus.Open)
            .OrderBy(o => o.SequenceNumber)
            .ToListAsync();

        return Results.Ok(openings);
    }

    [ResourceAction("Application", "Feedback", "Add interview feedback")]
    private static async Task<IResult> AddFeedback(
        Guid id,
        AddFeedbackRequest request,
        AuthUser authUser,
        MicroDbContext db)
    {
        var application = await db.Applications.FindAsync(id);
        if (application is null) return Results.NotFound();

        if (string.IsNullOrWhiteSpace(request.Notes))
        {
            return Results.BadRequest("Notes are required.");
        }

        if (request.Score < 1 || request.Score > 5)
        {
            return Results.BadRequest("Score must be between 1 and 5.");
        }

        var feedback = new Micro.API.Data.Models.Feedback
        {
            Id = Guid.NewGuid(),
            ApplicationId = id,
            AdminId = authUser.Id,
            Notes = request.Notes,
            Score = request.Score,
            CreatedAt = DateTime.UtcNow
        };

        db.Feedbacks.Add(feedback);
        await db.SaveChangesAsync();

        return Results.Created($"/api/applications/{id}/feedback", new { feedback.Id });
    }

    [ResourceAction("Application", "Edit", "Update interview details")]
    private static async Task<IResult> UpdateInterviewDetails(Guid id, UpdateInterviewDetailsRequest request, MicroDbContext db)
    {
        var application = await db.Applications.FindAsync(id);
        if (application is null) return Results.NotFound();

        if (request.CustomFieldValues is not null)
        {
            var cfErrors = await CustomFieldPersistence.ValidateApplicationAsync(
                db, application.Id, application.Status, request.CustomFieldValues, CancellationToken.None);
            if (cfErrors is not null)
                return Results.ValidationProblem(cfErrors);
        }

        if (application.Interview == null)
        {
            application.Interview = new InterviewDetails();
        }
        
        application.Interview.ScheduledDate = request.ScheduledDate;
        application.Interview.InterviewerName = request.InterviewerName;
        application.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        if (request.CustomFieldValues is not null)
        {
            await CustomFieldPersistence.PersistApplicationAsync(
                db, application.Id, application.Status, request.CustomFieldValues, CancellationToken.None);
            await db.SaveChangesAsync();
        }

        return Results.NoContent();
    }

    [ResourceAction("Application", "Edit", "Update offer details")]
    private static async Task<IResult> UpdateOfferDetails(Guid id, UpdateOfferDetailsRequest request, MicroDbContext db)
    {
        var application = await db.Applications.FindAsync(id);
        if (application is null) return Results.NotFound();

        if (request.CustomFieldValues is not null)
        {
            var cfErrors = await CustomFieldPersistence.ValidateApplicationAsync(
                db, application.Id, application.Status, request.CustomFieldValues, CancellationToken.None);
            if (cfErrors is not null)
                return Results.ValidationProblem(cfErrors);
        }

        if (application.Offer == null)
        {
            application.Offer = new OfferDetails();
        }

        application.Offer.ProposedSalary = request.ProposedSalary;
        application.Offer.TargetStartDate = request.TargetStartDate;
        application.Offer.Deadline = request.Deadline;
        application.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        if (request.CustomFieldValues is not null)
        {
            await CustomFieldPersistence.PersistApplicationAsync(
                db, application.Id, application.Status, request.CustomFieldValues, CancellationToken.None);
            await db.SaveChangesAsync();
        }

        return Results.NoContent();
    }
}

public record UpdateInterviewDetailsRequest(DateTime? ScheduledDate, string? InterviewerName, List<CustomFieldValueInput>? CustomFieldValues = null);
public record UpdateOfferDetailsRequest(decimal? ProposedSalary, DateTime? TargetStartDate, DateTime? Deadline, List<CustomFieldValueInput>? CustomFieldValues = null);
