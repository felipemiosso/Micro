namespace Micro.API.Data.Models;

public class RequisitionOpening
{
    public Guid Id { get; set; }
    public Guid RequisitionId { get; set; }
    public int SequenceNumber { get; set; }
    public DateTime? TargetStartDate { get; set; }
    public Guid? CandidateId { get; set; }
    public OpeningStatus Status { get; set; } = OpeningStatus.Open;

    // Navigation
    public Requisition Requisition { get; set; } = null!;
    public Candidate? Candidate { get; set; }
}

public enum OpeningStatus
{
    Open,
    Filled,
    Cancelled
}
