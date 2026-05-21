namespace Micro.API.Data.Models;

public class Application
{
    public Guid Id { get; set; }
    public Guid JobPostingId { get; set; }
    public Guid CandidateId { get; set; }
    public string ResumePath { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public ArchivalResolution ArchivalResolution { get; set; } = ArchivalResolution.None;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public JobPosting JobPosting { get; set; } = null!;
    public Candidate Candidate { get; set; } = null!;
    public Guid? RequisitionOpeningId { get; set; }
    public RequisitionOpening? RequisitionOpening { get; set; }
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    
    public InterviewDetails? Interview { get; set; }
    public OfferDetails? Offer { get; set; }
}

public enum ApplicationStatus
{
    Applied,
    Interview,
    Offer,
    Archive
}

public enum ArchivalResolution
{
    None,
    Hired,
    Rejected,
    Declined,
    Withdrawn
}
