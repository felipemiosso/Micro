# Backend Design - Requisition Management

## Data Model

### Requisition Entity
```csharp
namespace Micro.API.Data.Models;

public class Requisition
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Openings { get; set; }
    public RequisitionStatus Status { get; set; } = RequisitionStatus.Draft;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinalizedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public enum RequisitionStatus
{
    Draft,
    Finalized,
    Closed
}
```

## API Contracts

### GET /api/requisitions
- **Description:** Returns all requisitions.
- **Auth:** Admin only.
- **Response:** `200 OK` with `IEnumerable<RequisitionResponse>`.

### POST /api/requisitions
- **Request:** `CreateRequisitionRequest { Title, Department, Openings }`
- **Response:** `201 Created`.

### PUT /api/requisitions/{id}
- **Request:** `UpdateRequisitionRequest { Title, Department, Openings }`
- **Logic:** Validate status is `Draft`. Return `400 Bad Request` if not.

### POST /api/requisitions/{id}/finalize
- **Logic:** Update status to `Finalized` and set `FinalizedAt`.
- **Note:** In Feature 003, this will trigger Job Posting creation.

### POST /api/requisitions/{id}/close
- **Logic:** Update status to `Closed` and set `ClosedAt`.

## Infrastructure
- Update `MicroDbContext` to include `DbSet<Requisition>`.
- Create a new migration for the Requisition table.
