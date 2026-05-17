using Microsoft.AspNetCore.Identity;

namespace Micro.API.Data.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}
