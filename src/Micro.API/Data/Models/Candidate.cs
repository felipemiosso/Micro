namespace Micro.API.Data.Models;

public class Candidate
{
    public Guid Id { get; set; }
    
    public string FullName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string? Phone { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
