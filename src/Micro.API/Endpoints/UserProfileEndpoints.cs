using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Micro.API.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Micro.API.Endpoints;

public static class UserProfileEndpoints
{
    public static void MapUserProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, UserManager<AppUser> userManager) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Results.Unauthorized();

            var appUser = await userManager.FindByIdAsync(userId);
            if (appUser == null) return Results.NotFound();

            return Results.Ok(new UserProfileResponse(
                appUser.Id,
                appUser.Email!,
                appUser.FullName,
                appUser.PhotoUrl
            ));
        });
    }

    public record UserProfileResponse(string Id, string Email, string FullName, string? PhotoUrl);
}
