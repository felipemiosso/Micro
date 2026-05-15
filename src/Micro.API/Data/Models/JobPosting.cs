namespace Micro.API.Data.Models;

public class JobPosting
{
    public Guid Id { get; set; }
    public Guid RequisitionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public JobPostingStatus Status { get; set; } = JobPostingStatus.Published;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Navigation property
    public Requisition Requisition { get; set; } = null!;
}

public enum JobPostingStatus
{
    Published,
    Closed
}
