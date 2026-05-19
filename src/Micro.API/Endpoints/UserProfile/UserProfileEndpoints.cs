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

        group.MapGet("/", GetProfile).RequireAuthorization("Profile:View");
        group.MapPost("/", SyncProfile).RequireAuthorization("Profile:Sync");
    }

    [ResourceAction("Profile", "View", "View current user profile")]
    private static async Task<IResult> GetProfile(AuthUser authUser, MicroDbContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == authUser.Id);
        if (user == null) return Results.NotFound();

        return Results.Ok(new UserProfileResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.PhotoUrl,
            authUser.Roles,
            authUser.Permissions
        ));
    }

    [ResourceAction("Profile", "Sync", "Sync user info from identity provider")]
    private static async Task<IResult> SyncProfile(AuthUser authUser, MicroDbContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == authUser.Id);
        
        if (user == null)
        {
            user = new Data.Models.User
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
            user.Email = authUser.Email;
            user.FullName = authUser.Name ?? user.FullName;
            user.PhotoUrl = authUser.PhotoUrl ?? user.PhotoUrl;
            
            // Clear pending invite flag upon successful login/sync
            if (user.IsInvitePending)
            {
                user.IsInvitePending = false;
            }
        }

        try 
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            dbContext.Entry(user).State = EntityState.Detached;
            user = await dbContext.Users.FirstAsync(u => u.Id == authUser.Id);
        }

        return Results.Ok(new UserProfileResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.PhotoUrl,
            authUser.Roles,
            authUser.Permissions
        ));
    }
}
