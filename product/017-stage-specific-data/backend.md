# 017: Stage-Specific Application Data - Backend Design

## Schema Changes
We will expand the `Application` entity using owned entities to securely store stage-specific data.

### EF Entities
Add new owned types to `Micro.API.Data.Models.Application`:
```csharp
public class InterviewDetails
{
    public DateTime? ScheduledDate { get; set; }
    public string? InterviewerName { get; set; }
}

public class OfferDetails
{
    public decimal? ProposedSalary { get; set; }
    public DateTime? TargetStartDate { get; set; }
    public DateTime? Deadline { get; set; }
}
```
Update `Application` to include `InterviewDetails? Interview` and `OfferDetails? Offer`.
Configure these as `.OwnsOne()` in the EF Core configuration.

## API Contracts
Update `ApplicationDto` to return these new structures so the frontend can bind to them.

### Update Endpoints
Introduce specific endpoints to manage stage data:
- `PUT /api/applications/{id}/interview-details`
  - Body: `{ scheduledDate, interviewerName }`
- `PUT /api/applications/{id}/offer-details`
  - Body: `{ proposedSalary, targetStartDate, deadline }`

**Validations**:
- Offer endpoints should ideally validate that the application is at least in the `Offer` stage (or allow preparing an offer while in Interview, depending on business rule clarification).
- Ensure `decimal` serialization works correctly for the frontend.
