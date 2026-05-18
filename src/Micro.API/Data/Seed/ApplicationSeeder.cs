using Bogus;
using Micro.API.Data.Models;

namespace Micro.API.Data.Seed;

public static class ApplicationSeeder
{
    public static (List<Candidate> Candidates, List<Application> Applications) Generate(List<JobPosting> jobPostings, int countPerJob = 5)
    {
        var candidates = new List<Candidate>();
        var applications = new List<Application>();

        var candidateFaker = new Faker<Candidate>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.FullName, f => f.Name.FullName())
            .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.FullName))
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(1).ToUniversalTime());

        var appFaker = new Faker<Application>()
            .RuleFor(a => a.Id, f => f.Random.Guid())
            .RuleFor(a => a.ResumePath, f => "/storage/resumes/fake-resume.pdf")
            .RuleFor(a => a.Status, f => f.PickRandom<ApplicationStatus>())
            .RuleFor(a => a.AppliedAt, f => f.Date.Recent(60).ToUniversalTime())
            .RuleFor(a => a.ArchivalResolution, (f, a) => 
                a.Status == ApplicationStatus.Archive 
                ? f.PickRandomWithout(ArchivalResolution.None) 
                : ArchivalResolution.None);

        foreach (var job in jobPostings)
        {
            for (int i = 0; i < countPerJob; i++)
            {
                var candidate = candidateFaker.Generate();
                candidates.Add(candidate);

                var app = appFaker.Generate();
                app.JobPostingId = job.Id;
                app.CandidateId = candidate.Id;
                applications.Add(app);
            }
        }

        return (candidates, applications);
    }
}
