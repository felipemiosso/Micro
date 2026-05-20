namespace Micro.API.Data.Models;

public class Requisition
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    
    // Normalized Lookups
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    
    public Guid SalaryBandId { get; set; }
    public SalaryBand SalaryBand { get; set; } = null!;
    
    public Guid CostCenterId { get; set; }
    public CostCenter CostCenter { get; set; } = null!;

    public int OpeningsCount { get; set; } // Renamed from Openings to avoid confusion with collection
    public RequisitionStatus Status { get; set; } = RequisitionStatus.Draft;

    // The Hiring Team
    public string CreatedBy { get; set; } = string.Empty;
    public Guid HiringManagerId { get; set; }
    public Guid? RecruiterId { get; set; }

    // Logistics & Metadata
    public EmploymentType EmploymentType { get; set; }
    public WorkplaceType WorkplaceType { get; set; }
    public string Location { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;

    public bool IsInternalOnly { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinalizedAt { get; set; } // Fixed missing property
    public DateTime? PublishedAt { get; set; }
    public DateTime? OfferAcceptedAt { get; set; }
    public DateTime? CandidateStartedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? TargetStartDate { get; set; }

    // Navigation
    public ICollection<RequisitionOpening> Openings { get; set; } = new List<RequisitionOpening>();
}

public enum RequisitionStatus
{
    Draft,
    Finalized,
    Closed
}

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Internship,
    Temporary,
    Volunteer
}

public enum WorkplaceType
{
    OnSite,
    Remote,
    Hybrid
}