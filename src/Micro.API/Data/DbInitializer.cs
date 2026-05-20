using Micro.API.Data.Models;
using Micro.API.Data.Seed;
using Microsoft.EntityFrameworkCore;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Micro.API.Data;

public static class DbInitializer
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<MicroDbContext>();
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        
        if (config.GetValue<bool>("Firebase:UseEmulator"))
        {
            await SeedFirebaseEmulator(config);
        }

        await SeedUser(serviceProvider);
        await SeedLookups(serviceProvider);

        if (await dbContext.Requisitions.AnyAsync())
        {
            return;
        }

        // 2. Seed more Users
        var users = UserSeeder.Generate(20);
        foreach (var user in users)
        {
            if (!await dbContext.Users.AnyAsync(u => u.Id == user.Id))
            {
                dbContext.Users.Add(user);
            }
        }
        await dbContext.SaveChangesAsync();
        var allUsers = await dbContext.Users.ToListAsync();

        var departments = await dbContext.Departments.ToListAsync();
        var bands = await dbContext.SalaryBands.ToListAsync();
        var costCenters = await dbContext.CostCenters.ToListAsync();

        // 3. Seed Requisitions and Job Postings
        var (requisitions, jobPostings) = RequisitionSeeder.Generate(allUsers, departments, bands, costCenters, 50);
        dbContext.Requisitions.AddRange(requisitions);
        dbContext.JobPostings.AddRange(jobPostings);
        await dbContext.SaveChangesAsync();

        // 4. Seed Candidates and Applications
        var (candidates, applications) = ApplicationSeeder.Generate(jobPostings, 10);
        dbContext.Candidates.AddRange(candidates);
        dbContext.Applications.AddRange(applications);
        await dbContext.SaveChangesAsync();

        // 5. Seed Feedback
        var feedbacks = FeedbackSeeder.Generate(applications, allUsers, 200);
        dbContext.Feedbacks.AddRange(feedbacks);
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedLookups(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<MicroDbContext>();
        if (await dbContext.Departments.AnyAsync()) return;

        var departments = LookupSeeder.GenerateDepartments();
        var bands = LookupSeeder.GenerateSalaryBands();
        var costCenters = LookupSeeder.GenerateCostCenters();
        
        dbContext.Departments.AddRange(departments);
        dbContext.SalaryBands.AddRange(bands);
        dbContext.CostCenters.AddRange(costCenters);
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedUser(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<MicroDbContext>();
        var testUserId = UserSeeder.TestUserId;
        var exists = await dbContext.Users.AnyAsync(u => u.Id == testUserId);
        
        if (!exists)
        {
            dbContext.Users.Add(new User
            {
                Id = testUserId,
                Email = "test@microats.com",
                FullName = "Test User",
            });
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedFirebaseEmulator(IConfiguration config)
    {
        var projectId = config["Firebase:ProjectId"] ?? "demo-micro-ats";
        Environment.SetEnvironmentVariable("FIREBASE_AUTH_EMULATOR_HOST", "localhost:9099");
        
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                ProjectId = projectId,
                Credential = GoogleCredential.FromAccessToken("mock-token")
            });
        }

        for (int i = 0; i < 5; i++)
        {
            try
            {
                await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
                {
                    Email = "admin@microats.com",
                    Password = "AdminPassword123!",
                    EmailVerified = true,
                    DisplayName = "Admin User"
                });

                var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync("admin@microats.com");
                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(user.Uid, new Dictionary<string, object>
                {
                    { "role", "Admin" }
                });
                return;
            }
            catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.EmailAlreadyExists)
            {
                return;
            }
            catch (Exception)
            {
                await Task.Delay(1000);
            }
        }
    }
}
