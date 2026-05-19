# Technical Design - User Invitations

## DB Schema & Models

**Update `User` Entity**
Add fields to track invitation state:
```csharp
public class User
{
    public string Id { get; set; } = string.Empty; // Firebase UID
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    
    // Invitation Tracking
    public bool IsInvitePending { get; set; } = false;
    public DateTime? InviteSentAt { get; set; }

    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
```

## API Endpoints (New `UserManagementEndpoints.cs`)

**POST `/api/users/invite`**
- Auth: `[ResourceAction("User", "Invite", "Invite new users")]`
- Payload: `Email`, `FullName`, `List<Guid> RoleIds`
- Action: 
  1. Uses Firebase Admin SDK to create user (generates random password).
  2. Generates password reset link via Firebase Admin SDK.
  3. Uses Firebase Admin SDK to send the reset email (or we return link to log for MVP).
  4. Creates local `User` record with `IsInvitePending = true` and assigns Roles.

**POST `/api/users/{userId}/resend-invite`**
- Auth: `[ResourceAction("User", "Invite", "Invite new users")]`
- Action: Generates and sends a new password reset link.

**GET `/api/users`**
- Auth: `[ResourceAction("User", "View", "List users")]`
- Response: List of users with their roles and invite status.

**PUT `/api/users/{userId}/roles`**
- Auth: `[ResourceAction("User", "ManageRoles", "Manage user roles")]`
- Payload: `List<Guid> RoleIds`
- Action: Updates user's roles.

## Note on "Revoke"
With Firebase Admin, to revoke an invite, we either delete the Firebase Auth user entirely and remove the local DB record, or we disable the Firebase account. We will implement "Delete User" which completely removes them if they haven't set up yet.

**DELETE `/api/users/{userId}`**
- Auth: `[ResourceAction("User", "Delete", "Delete users")]`
- Action: Deletes from Firebase Auth and local DB.
