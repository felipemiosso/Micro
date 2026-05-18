using Micro.API.Data.Models;

namespace Micro.API.Endpoints.Application;

public record UpdateStatusRequest(ApplicationStatus Status, ArchivalResolution Resolution);
public record AddFeedbackRequest(string Notes, int Score);
