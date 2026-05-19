using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Micro.API.Endpoints.User;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        group.MapPost("/invite", InviteUser).RequireAuthorization("User:Invite");
        group.MapPost("/{userId}/resend-invite", ResendInvite).RequireAuthorization("User:Invite");
        group.MapGet("/", GetUsers).RequireAuthorization("User:View");
        group.MapPut("/{userId}/roles", ManageRoles).RequireAuthorization("User:ManageRoles");
        group.MapDelete("/{userId}", DeleteUser).RequireAuthorization("User:Delete");
    }

    [ResourceAction("User", "Invite", "Invite new users")]
    private static async Task<IResult> InviteUser([FromBody] InviteUserRequest request, MicroDbContext db, ILogger<InviteUserRequest> logger)
    {
        // 1. Check if user already exists in DB
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return Results.BadRequest(new { Message = "Email is already registered or invited." });
        }

        var auth = FirebaseAuth.DefaultInstance;
        UserRecord? userRecord = null;

        try
        {
            // Try to get user from Firebase Auth first (they might exist but not in our DB)
            userRecord = await auth.GetUserByEmailAsync(request.Email);
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            // User does not exist in Firebase, create them
            var userArgs = new UserRecordArgs
            {
                Email = request.Email,
                EmailVerified = false,
                DisplayName = request.FullName,
                Password = Guid.NewGuid().ToString() // Random placeholder password
            };
            userRecord = await auth.CreateUserAsync(userArgs);
        }

        // Generate password reset link
        var resetLink = await auth.GeneratePasswordResetLinkAsync(request.Email);
        
        // Log the link (simulate sending email)
        logger.LogInformation("===========================================");
        logger.LogInformation("INVITATION SENT TO: {Email}", request.Email);
        logger.LogInformation("PASSWORD RESET LINK: {Link}", resetLink);
        logger.LogInformation("===========================================");

        // Create local DB record
        var user = new Data.Models.User
        {
            Id = userRecord!.Uid,
            Email = request.Email,
            FullName = request.FullName,
            IsInvitePending = true,
            InviteSentAt = DateTime.UtcNow
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

    [ResourceAction("User", "Invite", "Invite new users")]
    private static async Task<IResult> ResendInvite(string userId, MicroDbContext db, ILogger<InviteUserRequest> logger)
    {
        var user = await db.Users.FindAsync(userId);
        if (user == null) return Results.NotFound();
        if (!user.IsInvitePending) return Results.BadRequest(new { Message = "User is already active." });

        var auth = FirebaseAuth.DefaultInstance;
        var resetLink = await auth.GeneratePasswordResetLinkAsync(user.Email);
        
        user.InviteSentAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation("===========================================");
        logger.LogInformation("RESENT INVITATION TO: {Email}", user.Email);
        logger.LogInformation("PASSWORD RESET LINK: {Link}", resetLink);
        logger.LogInformation("===========================================");

        return Results.NoContent();
    }

    [ResourceAction("User", "View", "List users")]
    private static async Task<IResult> GetUsers(MicroDbContext db)
    {
        var users = await db.Users
            .Include(u => u.Roles)
            .Select(u => new UserResponse(
                u.Id,
                u.Email,
                u.FullName,
                u.IsInvitePending,
                u.InviteSentAt,
                u.Roles.Select(r => new RoleDto(r.Id, r.Name)).ToList()
            ))
            .ToListAsync();

        return Results.Ok(users);
    }

    [ResourceAction("User", "ManageRoles", "Manage user roles")]
    private static async Task<IResult> ManageRoles(string userId, [FromBody] ManageUserRolesRequest request, MicroDbContext db)
    {
        var user = await db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return Results.NotFound();

        user.Roles.Clear();
        if (request.RoleIds != null && request.RoleIds.Any())
        {
            var roles = await db.Roles.Where(r => request.RoleIds.Contains(r.Id)).ToListAsync();
            foreach (var role in roles)
            {
                user.Roles.Add(role);
            }
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    [ResourceAction("User", "Delete", "Delete users")]
    private static async Task<IResult> DeleteUser(string userId, MicroDbContext db)
    {
        var user = await db.Users.FindAsync(userId);
        if (user == null) return Results.NotFound();

        // Only allow deleting pending invites for now, or full delete?
        // Spec says "removing their access and record from the system."
        
        try
        {
            var auth = FirebaseAuth.DefaultInstance;
            await auth.DeleteUserAsync(userId);
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            // Ignore if not in Firebase
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}

public record InviteUserRequest(string Email, string FullName, List<Guid>? RoleIds);
public record ManageUserRolesRequest(List<Guid> RoleIds);
public record RoleDto(Guid Id, string Name);
public record UserResponse(string Id, string Email, string FullName, bool IsInvitePending, DateTime? InviteSentAt, List<RoleDto> Roles);
