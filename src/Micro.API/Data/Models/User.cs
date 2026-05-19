namespace Micro.API.Data.Models;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }

    // Invitation Tracking
    public bool IsInvitePending { get; set; } = false;
    public DateTime? InviteSentAt { get; set; }

    // Many-to-many relationship
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
