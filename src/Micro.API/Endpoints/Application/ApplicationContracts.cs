using Micro.API.Data.Models;
using Micro.API.Endpoints.CustomFields;
using System.Collections.Generic;

namespace Micro.API.Endpoints.Application;

public record UpdateStatusRequest(ApplicationStatus Status, ArchivalResolution Resolution, Guid? RequisitionOpeningId = null, List<CustomFieldValueInput>? CustomFieldValues = null);
public record AddFeedbackRequest(string Notes, int Score);
