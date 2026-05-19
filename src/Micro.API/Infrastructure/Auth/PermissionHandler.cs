using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Micro.API.Data;

namespace Micro.API.Infrastructure.Auth;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    public PermissionHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst("user_id")?.Value;
        if (userId == null) return;

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MicroDbContext>();

        // Check if user has the required permission via their roles
        var hasPermission = await dbContext.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .AnyAsync(r => r.Permissions.Contains(requirement.Permission));

        // Special case for Admin role from token (bootstrap)
        if (!hasPermission)
        {
            var isAdmin = context.User.FindFirst("role")?.Value == "Admin" || 
                          context.User.IsInRole("Admin");
            if (isAdmin) hasPermission = true;
        }

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
