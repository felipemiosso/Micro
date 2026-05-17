using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Data;

public static class DbInitializer
{
    public static async Task SeedUser(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<MicroDbContext>();
        
        // We don't strictly need to seed users anymore as they are managed by Firebase
        // and created in our DB on their first request to /api/profile.
        // However, we can seed a test user for development/testing if needed.
        
        var testUserId = "test-user-id";
        var exists = await dbContext.Users.AnyAsync(u => u.Id == testUserId);
        
        if (!exists)
        {
            dbContext.Users.Add(new AppUser
            {
                Id = testUserId,
                Email = "test@microats.com",
                FullName = "Test User"
            });
            await dbContext.SaveChangesAsync();
        }
    }
}
