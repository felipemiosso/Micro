namespace Micro.API.Endpoints.UserProfile;

public record UserProfileResponse(string Id, string Email, string FullName, string? PhotoUrl);
