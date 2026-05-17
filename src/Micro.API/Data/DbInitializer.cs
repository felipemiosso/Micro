using Micro.API.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Micro.API.Data;

public static class DbInitializer
{
    public static async Task SeedAdminUser(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var adminEmail = "admin@microats.com";
        var adminPassword = "AdminPassword123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Admin"
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded && result.Errors.All(e => e.Code != "DuplicateUserName" && e.Code != "DuplicateEmail"))
            {
                throw new Exception($"Failed to seed admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
