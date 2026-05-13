namespace Micro.API.Data.Models;

public class Requisition
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Openings { get; set; }
    public RequisitionStatus Status { get; set; } = RequisitionStatus.Draft;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinalizedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public enum RequisitionStatus
{
    Draft,
    Finalized,
    Closed
}
