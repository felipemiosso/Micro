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
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == authUser.Id);
            if (user == null) return Results.NotFound();

            return Results.Ok(new UserProfileResponse(
                user.Id,
                user.Email,
                user.FullName,
                user.PhotoUrl
            ));
        });

        group.MapPost("/", async (AuthUser authUser, MicroDbContext dbContext) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == authUser.Id);
            
            if (user == null)
            {
                user = new User
                {
                    Id = authUser.Id,
                    Email = authUser.Email,
                    FullName = authUser.Name ?? authUser.Email.Split('@')[0],
                    PhotoUrl = authUser.PhotoUrl
                };
                dbContext.Users.Add(user);
            }
            else
            {
                // Update existing user info if it changed in Firebase
                user.Email = authUser.Email;
                user.FullName = authUser.Name ?? user.FullName;
                user.PhotoUrl = authUser.PhotoUrl ?? user.PhotoUrl;
            }

            try 
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // If insert failed due to race condition, the user now exists.
                // We don't need to do anything as the winner of the race created the user.
                // We re-fetch to ensure we return the correct state.
                dbContext.Entry(user).State = EntityState.Detached;
                user = await dbContext.Users.FirstAsync(u => u.Id == authUser.Id);
            }

            return Results.Ok(new UserProfileResponse(
                user.Id,
                user.Email,
                user.FullName,
                user.PhotoUrl
            ));
        });
    }
}
