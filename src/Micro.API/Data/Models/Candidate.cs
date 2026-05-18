using System.ComponentModel.DataAnnotations;

namespace Micro.API.Data.Models;

public class Candidate
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Phone { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
