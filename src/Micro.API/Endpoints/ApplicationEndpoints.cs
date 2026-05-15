using Micro.API.Data;
using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints;

public static class ApplicationEndpoints
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        // Public endpoint
        app.MapPost("/api/public/jobs/{jobPostingId:guid}/apply", ApplyToJob).DisableAntiforgery();

        // Admin endpoints
        var adminGroup = app.MapGroup("/api/admin/applications").RequireAuthorization();
        adminGroup.MapGet("/", GetAdminApplications);
        adminGroup.MapGet("/{id:guid}/resume", GetApplicationResume);
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

        if (resumeFile.Length > 5 * 1024 * 1024)
        {
            return Results.BadRequest("File too large. Max 5MB allowed.");
        }

        if (Path.GetExtension(resumeFile.FileName).ToLower() != ".pdf")
        {
            return Results.BadRequest("Only PDF files are allowed.");
        }

        // Duplicate check
        var exists = await db.Applications.AnyAsync(a => a.JobPostingId == jobPostingId && a.CandidateEmail == email);
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

        var application = new Application
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPostingId,
            CandidateName = name,
            CandidateEmail = email,
            CandidatePhone = string.IsNullOrWhiteSpace(phone) ? null : phone,
            ResumePath = filePath,
            AppliedAt = DateTime.UtcNow,
            Status = ApplicationStatus.Applied
        };

        db.Applications.Add(application);
        await db.SaveChangesAsync();

        return Results.Created($"/api/admin/applications/{application.Id}", new { application.Id });
    }

    private static async Task<IResult> GetAdminApplications(Guid? jobPostingId, MicroDbContext db)
    {
        var query = db.Applications.Include(a => a.JobPosting).AsQueryable();
        if (jobPostingId.HasValue)
        {
            query = query.Where(a => a.JobPostingId == jobPostingId.Value);
        }

        var applications = await query
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new {
                a.Id,
                a.JobPostingId,
                JobTitle = a.JobPosting.Title,
                a.CandidateName,
                a.CandidateEmail,
                a.CandidatePhone,
                a.Status,
                a.AppliedAt
            })
            .ToListAsync();

        return Results.Ok(applications);
    }

    private static async Task<IResult> GetApplicationResume(Guid id, MicroDbContext db)
    {
        var application = await db.Applications.FindAsync(id);
        if (application is null) return Results.NotFound();

        if (!File.Exists(application.ResumePath))
        {
            return Results.NotFound("Resume file not found on server.");
        }

        var bytes = await File.ReadAllBytesAsync(application.ResumePath);
        return Results.File(bytes, "application/pdf", $"{application.CandidateName}_Resume.pdf");
    }
}
