using Bogus;
using Micro.API.Data.Models;

namespace Micro.API.Data.Seed;

public static class UserSeeder
{
    public const string TestUserId = "test-user-id";

    public static List<User> Generate(int count = 5)
    {
        var faker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid().ToString())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.PhotoUrl, f => f.Internet.Avatar());

        var users = faker.Generate(count);
        
        // Add default test user if not present
        users.Insert(0, new User
        {
            Id = TestUserId,
            Email = "test@microats.com",
            FullName = "Test User"
        });

        return users;
    }
}
