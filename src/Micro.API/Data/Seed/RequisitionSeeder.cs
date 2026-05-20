using Bogus;
using Micro.API.Data.Models;

namespace Micro.API.Data.Seed;

public static class RequisitionSeeder
{
    public static (List<Requisition>, List<JobPosting>) Generate(
        List<User> users, 
        List<Department> departments,
        List<SalaryBand> bands,
        List<CostCenter> costCenters,
        int count = 10)
    {
        var requisitions = new List<Requisition>();
        var jobPostings = new List<JobPosting>();

        var userIds = users.Select(u => u.Id).ToList();

        var reqFaker = new Faker<Requisition>()
            .RuleFor(r => r.Id, f => f.Random.Guid())
            .RuleFor(r => r.Title, f => f.Name.JobTitle())
            .RuleFor(r => r.DepartmentId, f => f.PickRandom(departments).Id)
            .RuleFor(r => r.SalaryBandId, f => f.PickRandom(bands).Id)
            .RuleFor(r => r.CostCenterId, f => f.PickRandom(costCenters).Id)
            .RuleFor(r => r.OpeningsCount, f => f.Random.Number(1, 5))
            .RuleFor(r => r.Status, f => f.PickRandom<RequisitionStatus>())
            .RuleFor(r => r.EmploymentType, f => f.PickRandom<EmploymentType>())
            .RuleFor(r => r.WorkplaceType, f => f.PickRandom<WorkplaceType>())
            .RuleFor(r => r.Location, f => f.Address.City())
            .RuleFor(r => r.JobDescription, f => f.Lorem.Paragraphs(2))
            .RuleFor(r => r.CreatedBy, f => f.PickRandom(userIds).ToString())
            .RuleFor(r => r.CreatedAt, f => f.Date.Past(1).ToUniversalTime());

        var jobFaker = new Faker<JobPosting>()
            .RuleFor(j => j.Id, f => f.Random.Guid())
            .RuleFor(j => j.Description, f => f.Lorem.Paragraphs(3))
            .RuleFor(j => j.Requirements, f => string.Join("\n", f.Lorem.Sentences(5)))
            .RuleFor(j => j.Status, f => f.PickRandom<JobPostingStatus>())
            .RuleFor(j => j.CreatedAt, (f, j) => f.Date.Recent(30).ToUniversalTime());

        for (int i = 0; i < count; i++)
        {
            var req = reqFaker.Generate();
            
            // Initialize Openings
            for (int j = 1; j <= req.OpeningsCount; j++)
            {
                req.Openings.Add(new RequisitionOpening
                {
                    Id = Guid.NewGuid(),
                    SequenceNumber = j,
                    TargetStartDate = DateTime.UtcNow.AddMonths(1),
                    Status = OpeningStatus.Open
                });
            }

            requisitions.Add(req);

            if (req.Status != RequisitionStatus.Draft)
            {
                var job = jobFaker.Generate();
                job.RequisitionId = req.Id;
                job.Title = req.Title;
                
                if (req.Status == RequisitionStatus.Closed)
                {
                    job.Status = JobPostingStatus.Closed;
                    job.ClosedAt = req.ClosedAt ?? DateTime.UtcNow;
                }
                
                jobPostings.Add(job);
            }
        }

        return (requisitions, jobPostings);
    }
}
