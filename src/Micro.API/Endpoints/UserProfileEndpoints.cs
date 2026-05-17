using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Micro.API.Data;
using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints;

public static class UserProfileEndpoints
{
    public static void MapUserProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, MicroDbContext dbContext) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Results.Unauthorized();

            var appUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (appUser == null) return Results.NotFound();

            return Results.Ok(new UserProfileResponse(
                appUser.Id,
                appUser.Email,
                appUser.FullName,
                appUser.PhotoUrl
            ));
        });

        group.MapPost("/", async (ClaimsPrincipal user, MicroDbContext dbContext) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = user.FindFirstValue(ClaimTypes.Email);
            
            if (userId == null || email == null) return Results.BadRequest("Missing required user claims.");

            var appUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            
            if (appUser == null)
            {
                appUser = new AppUser
                {
                    Id = userId,
                    Email = email,
                    FullName = user.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0],
                    PhotoUrl = user.FindFirstValue("picture") // Firebase photo URL claim
                };
                dbContext.Users.Add(appUser);
            }
            else
            {
                // Update existing user info if it changed in Firebase
                appUser.Email = email;
                appUser.FullName = user.FindFirstValue(ClaimTypes.Name) ?? appUser.FullName;
                appUser.PhotoUrl = user.FindFirstValue("picture") ?? appUser.PhotoUrl;
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

    public record UserProfileResponse(string Id, string Email, string FullName, string? PhotoUrl);
}
