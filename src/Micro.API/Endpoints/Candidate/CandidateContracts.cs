namespace Micro.API.Endpoints.Candidate;

public record CandidateDetailResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    DateTime CreatedAt,
    List<CandidateApplicationResponse> Applications
);

public record CandidateApplicationResponse(
    Guid Id,
    Guid JobPostingId,
    string JobTitle,
    string Status,
    string ArchivalResolution,
    DateTime AppliedAt,
    List<CandidateFeedbackResponse> Feedbacks,
    Guid? RequisitionOpeningId = null,
    int? OpeningSequenceNumber = null,
    string? RequisitionTitle = null
);

public record CandidateFeedbackResponse(
    Guid Id,
    string Notes,
    int Score,
    DateTime CreatedAt
);
