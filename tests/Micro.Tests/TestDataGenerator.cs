using Bogus;
using Micro.API.Data.Models;

namespace Micro.Tests;

public static class TestDataGenerator
{
    private static readonly string[] Departments = { "Engineering", "Product", "Design", "Sales", "Marketing", "HR" };

    public static Requisition CreateRequisition(string createdBy = "test-user-id")
    {
        return new Faker<Requisition>()
            .RuleFor(r => r.Id, f => f.Random.Guid())
            .RuleFor(r => r.Title, f => f.Name.JobTitle())
            .RuleFor(r => r.Department, f => f.PickRandom(Departments))
            .RuleFor(r => r.Openings, f => f.Random.Number(1, 5))
            .RuleFor(r => r.Status, RequisitionStatus.Draft)
            .RuleFor(r => r.CreatedBy, createdBy)
            .RuleFor(r => r.CreatedAt, DateTime.UtcNow)
            .Generate();
    }

    public static JobPosting CreateJobPosting(Guid requisitionId, string title)
    {
        return new Faker<JobPosting>()
            .RuleFor(j => j.Id, f => f.Random.Guid())
            .RuleFor(j => j.RequisitionId, requisitionId)
            .RuleFor(j => j.Title, title)
            .RuleFor(j => j.Description, f => f.Lorem.Paragraph())
            .RuleFor(j => j.Requirements, f => f.Lorem.Sentence())
            .RuleFor(j => j.Status, JobPostingStatus.Published)
            .RuleFor(j => j.CreatedAt, DateTime.UtcNow)
            .Generate();
    }

    public static Application CreateApplication(Guid jobPostingId)
    {
        return new Faker<Application>()
            .RuleFor(a => a.Id, f => f.Random.Guid())
            .RuleFor(a => a.JobPostingId, jobPostingId)
            .RuleFor(a => a.CandidateName, f => f.Name.FullName())
            .RuleFor(a => a.CandidateEmail, (f, a) => f.Internet.Email(a.CandidateName))
            .RuleFor(a => a.ResumePath, "test/path/resume.pdf")
            .RuleFor(a => a.Status, ApplicationStatus.Applied)
            .RuleFor(a => a.AppliedAt, DateTime.UtcNow)
            .Generate();
    }
}
