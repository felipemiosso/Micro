using Micro.API.Data.Models;
using Micro.API.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Data;

public static class DbInitializer
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<MicroDbContext>();
        
        // Always ensure the test user exists
        await SeedUser(serviceProvider);

        // Only seed more data if the database is otherwise empty
        if (await dbContext.Requisitions.AnyAsync())
        {
            return;
        }

        // 1. Seed more Users
        var users = UserSeeder.Generate(20);
        foreach (var user in users)
        {
            if (!await dbContext.Users.AnyAsync(u => u.Id == user.Id))
            {
                dbContext.Users.Add(user);
            }
        }
        await dbContext.SaveChangesAsync();

        // Reload users to ensure we have them all
        var allUsers = await dbContext.Users.ToListAsync();

        // 2. Seed Requisitions and Job Postings
        var (requisitions, jobPostings) = RequisitionSeeder.Generate(allUsers, 50);
        dbContext.Requisitions.AddRange(requisitions);
        dbContext.JobPostings.AddRange(jobPostings);
        await dbContext.SaveChangesAsync();

        // 3. Seed Candidates and Applications
        var (candidates, applications) = ApplicationSeeder.Generate(jobPostings, 10);
        dbContext.Candidates.AddRange(candidates);
        dbContext.Applications.AddRange(applications);
        await dbContext.SaveChangesAsync();

        // 4. Seed Feedback
        var feedbacks = FeedbackSeeder.Generate(applications, allUsers, 200);
        dbContext.Feedbacks.AddRange(feedbacks);
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedUser(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<MicroDbContext>();
        var testUserId = UserSeeder.TestUserId;
        var exists = await dbContext.Users.AnyAsync(u => u.Id == testUserId);
        
        if (!exists)
        {
            dbContext.Users.Add(new AppUser
            {
                Id = testUserId,
                Email = "test@microats.com",
                FullName = "Test User",
            });
            await dbContext.SaveChangesAsync();
        }
    }
}
