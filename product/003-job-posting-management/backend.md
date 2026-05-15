# Backend Design - Job Posting Management

## Data Model

### JobPosting Entity
```csharp
namespace Micro.API.Data.Models;

public class JobPosting
{
    public Guid Id { get; set; }
    public Guid RequisitionId { get; set; }
    public string Title { get; set; } = string.Empty; // Initially copied from Requisition
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public JobPostingStatus Status { get; set; } = JobPostingStatus.Published;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Navigation property (optional, but useful)
    public Requisition Requisition { get; set; } = null!;
}

public enum JobPostingStatus
{
    Published,
    Closed
}
```

## API Contracts

### Public Endpoints (No Auth)

#### GET /api/jobs
- **Description:** Returns all "Published" job postings.
- **Response:** `200 OK` with `IEnumerable<PublicJobResponse>`.

#### GET /api/jobs/{id}
- **Description:** Returns details of a specific job posting.
- **Response:** `200 OK` with `PublicJobDetailResponse` or `404 Not Found`.

### Admin Endpoints (Auth Required)

#### GET /api/admin/jobs
- **Description:** Returns all job postings for management.
- **Response:** `200 OK` with `IEnumerable<JobPosting>`.

#### PUT /api/admin/jobs/{id}
- **Request:** `UpdateJobPostingRequest { Title, Description, Requirements }`
- **Response:** `204 No Content`.

#### POST /api/admin/jobs/{id}/close
- **Description:** Manually close a job posting.
- **Response:** `204 No Content`.

## Logic Integration

### Requisition Finalization
Modify `RequisitionEndpoints.FinalizeRequisition`:
1. After updating Requisition status to `Finalized`.
2. Check if a `JobPosting` already exists for this `RequisitionId`.
3. If not, create a new `JobPosting` with `Title` and `Department` (to be used in description) from the Requisition.

### Requisition Closure
Modify `RequisitionEndpoints.CloseRequisition`:
1. After updating Requisition status to `Closed`.
2. Find the associated `JobPosting`.
3. If found and its status is `Published`, set it to `Closed` and set `ClosedAt`.

## Infrastructure
- Update `MicroDbContext` to include `DbSet<JobPosting>`.
- Add configuration for `JobPosting` entity (e.g., relationship with `Requisition`).
- Create a new migration.
