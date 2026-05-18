namespace Micro.API.Endpoints.JobPosting;

public record PublicJobResponse(Guid Id, string Title, string Department, string Description, DateTime CreatedAt);
public record PublicJobDetailResponse(Guid Id, string Title, string Department, string Description, string Requirements, DateTime CreatedAt);
public record UpdateJobPostingRequest(string Title, string Description, string Requirements);
