using Micro.API.Data.Models;

namespace Micro.API.Endpoints.Application;

public record UpdateStatusRequest(ApplicationStatus Status, ArchivalResolution Resolution, Guid? RequisitionOpeningId = null);
public record AddFeedbackRequest(string Notes, int Score);
