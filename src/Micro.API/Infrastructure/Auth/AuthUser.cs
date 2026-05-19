using System.Security.Claims;
using Micro.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Infrastructure.Auth;

public record AuthUser(string Id, string Email, string? Name, string? PhotoUrl, List<string> Roles, List<string> Permissions)
{
    public static async ValueTask<AuthUser?> BindAsync(HttpContext context)
    {
        var userClaims = context.User;
        
        var id = userClaims.FindFirst("user_id")?.Value;
        var email = userClaims.FindFirst("email")?.Value;

        if (id is null || email is null)
        {
            return null;
        }

        var dbContext = context.RequestServices.GetRequiredService<MicroDbContext>();
        var dbUser = await dbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id);

        var roles = dbUser?.Roles.Select(r => r.Name).ToList() ?? new List<string>();
        var permissions = dbUser?.Roles.SelectMany(r => r.Permissions).Distinct().ToList() ?? new List<string>();

        // Special case for hardcoded Admin role from token if not in DB yet (for bootstrap)
        if (userClaims.FindFirst("role")?.Value == "Admin" || userClaims.FindFirst(ClaimTypes.Role)?.Value == "Admin")
        {
            if (!roles.Contains("Admin")) roles.Add("Admin");
            // Admin gets all permissions
            if (roles.Contains("Admin"))
            {
                var allActions = ActionDiscovery.GetAvailableActions();
                permissions = allActions.Select(a => a.Permission).Distinct().ToList();
            }
        }

        var name = userClaims.FindFirst("name")?.Value;
        var photoUrl = userClaims.FindFirst("picture")?.Value;

        return new AuthUser(id, email, name, photoUrl, roles, permissions);
    }

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}
