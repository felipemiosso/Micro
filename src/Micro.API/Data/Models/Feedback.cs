namespace Micro.API.Data.Models;

public class Feedback
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string AdminId { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Application Application { get; set; } = null!;
}
