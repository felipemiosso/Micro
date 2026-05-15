namespace Micro.API.Data.Models;

public class Application
{
    public Guid Id { get; set; }
    public Guid JobPostingId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CandidatePhone { get; set; }
    public string ResumePath { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public JobPosting JobPosting { get; set; } = null!;
}

public enum ApplicationStatus
{
    Applied,
    Interview,
    Offer,
    Archive
}
