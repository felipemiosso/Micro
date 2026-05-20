using Bogus;
using Micro.API.Data.Models;

namespace Micro.Tests;

public static class TestDataGenerator
{
    public static Requisition CreateRequisition(Guid departmentId, Guid salaryBandId, Guid costCenterId, string createdBy = "test-user-id")
    {
        return new Faker<Requisition>()
            .RuleFor(r => r.Id, f => f.Random.Guid())
            .RuleFor(r => r.Title, f => f.Name.JobTitle())
            .RuleFor(r => r.DepartmentId, departmentId)
            .RuleFor(r => r.SalaryBandId, salaryBandId)
            .RuleFor(r => r.CostCenterId, costCenterId)
            .RuleFor(r => r.OpeningsCount, f => f.Random.Number(1, 5))
            .RuleFor(r => r.Status, RequisitionStatus.Draft)
            .RuleFor(r => r.CreatedBy, createdBy)
            .RuleFor(r => r.CreatedAt, DateTime.UtcNow)
            .RuleFor(r => r.EmploymentType, f => f.PickRandom<EmploymentType>())
            .RuleFor(r => r.WorkplaceType, f => f.PickRandom<WorkplaceType>())
            .RuleFor(r => r.Location, f => f.Address.City())
            .RuleFor(r => r.JobDescription, f => f.Lorem.Paragraph())
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

    public static Candidate CreateCandidate()
    {
        return new Faker<Candidate>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.FullName, f => f.Name.FullName())
            .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.FullName))
            .RuleFor(c => c.CreatedAt, DateTime.UtcNow)
            .Generate();
    }

    public static Application CreateApplication(Guid jobPostingId, Guid candidateId)
    {
        return new Faker<Application>()
            .RuleFor(a => a.Id, f => f.Random.Guid())
            .RuleFor(a => a.JobPostingId, jobPostingId)
            .RuleFor(a => a.CandidateId, candidateId)
            .RuleFor(a => a.ResumePath, "test/path/resume.pdf")
            .RuleFor(a => a.Status, ApplicationStatus.Applied)
            .RuleFor(a => a.AppliedAt, DateTime.UtcNow)
            .Generate();
    }
}
