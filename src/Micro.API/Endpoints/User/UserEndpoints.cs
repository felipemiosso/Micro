using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints.User;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        group.MapPost("/invite", InviteUser).RequireAuthorization("User:Invite");
    }

    [ResourceAction("User", "Invite", "Invite a new employee and assign roles")]
    private static async Task<IResult> InviteUser(InviteUserRequest request, MicroDbContext db)
    {
        // For now, invitation just creates the user in our DB.
        // In a real app, this would send an email or interface with Firebase Auth.
        
        var user = new Data.Models.User
        {
            Id = Guid.NewGuid().ToString(), // Placeholder until they log in
            Email = request.Email,
            FullName = request.FullName
        };

        if (request.RoleIds != null && request.RoleIds.Any())
        {
            var roles = await db.Roles.Where(r => request.RoleIds.Contains(r.Id)).ToListAsync();
            foreach (var role in roles)
            {
                user.Roles.Add(role);
            }
        }

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Results.Created($"/api/users/{user.Id}", new { user.Id });
    }
}

public record InviteUserRequest(string Email, string FullName, List<Guid>? RoleIds);
