using Micro.API.Data;
using Micro.API.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints.Role;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles");

        group.MapGet("/", GetRoles).RequireAuthorization("Role:View");
        group.MapPost("/", CreateRole).RequireAuthorization("Role:Create");
        group.MapGet("/available-actions", GetAvailableActions).RequireAuthorization("Role:ViewActions");
    }

    [ResourceAction("Role", "View", "List existing roles")]
    private static async Task<IResult> GetRoles(MicroDbContext db)
    {
        var roles = await db.Roles
            .Select(r => new RoleResponse(r.Id, r.Name, r.Permissions))
            .ToListAsync();
        return Results.Ok(roles);
    }

    [ResourceAction("Role", "Create", "Create a new role")]
    private static async Task<IResult> CreateRole(CreateRoleRequest request, MicroDbContext db)
    {
        var role = new Data.Models.Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Permissions = request.Permissions
        };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        return Results.Created($"/api/roles/{role.Id}", new RoleResponse(role.Id, role.Name, role.Permissions));
    }

    [ResourceAction("Role", "ViewActions", "List all discoverable system actions")]
    private static IResult GetAvailableActions()
    {
        var actions = ActionDiscovery.GetAvailableActions();
        return Results.Ok(actions);
    }
}

public record RoleResponse(Guid Id, string Name, List<string> Permissions);
public record CreateRoleRequest(string Name, List<string> Permissions);
