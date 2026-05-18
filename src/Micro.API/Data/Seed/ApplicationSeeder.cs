using Bogus;
using Micro.API.Data.Models;

namespace Micro.API.Data.Seed;

public static class ApplicationSeeder
{
    public static List<Application> Generate(List<JobPosting> jobPostings, int countPerJob = 5)
    {
        var applications = new List<Application>();

        var appFaker = new Faker<Application>()
            .RuleFor(a => a.Id, f => f.Random.Guid())
            .RuleFor(a => a.CandidateName, f => f.Name.FullName())
            .RuleFor(a => a.CandidateEmail, (f, a) => f.Internet.Email(a.CandidateName))
            .RuleFor(a => a.CandidatePhone, f => f.Phone.PhoneNumber())
            .RuleFor(a => a.ResumePath, f => "/storage/resumes/fake-resume.pdf")
            .RuleFor(a => a.Status, f => f.PickRandom<ApplicationStatus>())
            .RuleFor(a => a.AppliedAt, f => f.Date.Recent(60).ToUniversalTime())
            .RuleFor(a => a.ArchivalResolution, (f, a) => 
                a.Status == ApplicationStatus.Archive 
                ? f.PickRandomWithout(ArchivalResolution.None) 
                : ArchivalResolution.None);

        foreach (var job in jobPostings)
        {
            var jobApps = appFaker.Generate(countPerJob);
            foreach (var app in jobApps)
            {
                app.JobPostingId = job.Id;
            }
            applications.AddRange(jobApps);
        }

        return applications;
    }
}
