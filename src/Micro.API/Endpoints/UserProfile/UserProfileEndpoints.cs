using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints.UserProfile;

public static class UserProfileEndpoints
{
    public static void MapUserProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile");

        group.MapGet("/", async (AuthUser authUser, MicroDbContext dbContext) =>
        {
            var appUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == authUser.Id);
            if (appUser == null) return Results.NotFound();

            return Results.Ok(new UserProfileResponse(
                appUser.Id,
                appUser.Email,
                appUser.FullName,
                appUser.PhotoUrl
            ));
        });

        group.MapPost("/", async (AuthUser authUser, MicroDbContext dbContext) =>
        {
            var appUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == authUser.Id);
            
            if (appUser == null)
            {
                appUser = new AppUser
                {
                    Id = authUser.Id,
                    Email = authUser.Email,
                    FullName = authUser.Name ?? authUser.Email.Split('@')[0],
                    PhotoUrl = authUser.PhotoUrl
                };
                dbContext.Users.Add(appUser);
            }
            else
            {
                // Update existing user info if it changed in Firebase
                appUser.Email = authUser.Email;
                appUser.FullName = authUser.Name ?? appUser.FullName;
                appUser.PhotoUrl = authUser.PhotoUrl ?? appUser.PhotoUrl;
            }

            await dbContext.SaveChangesAsync();

            return Results.Ok(new UserProfileResponse(
                appUser.Id,
                appUser.Email,
                appUser.FullName,
                appUser.PhotoUrl
            ));
        });
    }
}
