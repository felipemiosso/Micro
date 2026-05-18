using Bogus;
using Micro.API.Data.Models;

namespace Micro.API.Data.Seed;

public static class FeedbackSeeder
{
    public static List<Feedback> Generate(List<Application> applications, List<AppUser> users, int count = 20)
    {
        var userIds = users.Select(u => u.Id).ToList();
        
        var feedbackFaker = new Faker<Feedback>()
            .RuleFor(f => f.Id, f => f.Random.Guid())
            .RuleFor(f => f.ApplicationId, f => f.PickRandom(applications).Id)
            .RuleFor(f => f.AdminId, f => f.PickRandom(userIds))
            .RuleFor(f => f.Notes, f => f.Lorem.Sentence())
            .RuleFor(f => f.Score, f => f.Random.Number(1, 5))
            .RuleFor(f => f.CreatedAt, f => f.Date.Recent(30).ToUniversalTime());

        return feedbackFaker.Generate(count);
    }
}
