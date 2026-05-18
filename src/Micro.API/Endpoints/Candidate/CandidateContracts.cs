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
    int Status,
    int ArchivalResolution,
    DateTime AppliedAt,
    List<CandidateFeedbackResponse> Feedbacks
);

public record CandidateFeedbackResponse(
    Guid Id,
    string Notes,
    int Score,
    DateTime CreatedAt
);
