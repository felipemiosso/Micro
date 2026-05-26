# 019: Selectable Custom Fields — Backend Design

## Database Schema Changes

### `CustomFieldDefinition.cs` [Modified]
Add `IsGlobal` boolean property:
```csharp
public class CustomFieldDefinition
{
    // Existing fields...
    public bool IsGlobal { get; set; } = true; // Default true for backwards compatibility
}
```

### `RequisitionCustomField.cs` [NEW]
Join table representing selectable custom fields associated with specific Requisitions:
```csharp
namespace Micro.API.Data.Models;

public class RequisitionCustomField
{
    public Guid RequisitionId { get; set; }
    public Requisition Requisition { get; set; } = null!;
    
    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;
    
    public DateTime AssociatedAt { get; set; } = DateTime.UtcNow;
}
```

### `JobPostingCustomField.cs` [NEW]
Join table representing selectable custom fields (both Job Posting and Application types) associated with specific Job Postings:
```csharp
namespace Micro.API.Data.Models;

public class JobPostingCustomField
{
    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;
    
    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;
    
    public DateTime AssociatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## Entity Framework configurations

### `RequisitionCustomFieldConfiguration.cs` [NEW]
```csharp
using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class RequisitionCustomFieldConfiguration : IEntityTypeConfiguration<RequisitionCustomField>
{
    public void Configure(EntityTypeBuilder<RequisitionCustomField> builder)
    {
        builder.ToTable("RequisitionCustomFields");
        builder.HasKey(x => new { x.RequisitionId, x.CustomFieldDefinitionId });

        builder.HasOne(x => x.Requisition)
            .WithMany()
            .HasForeignKey(x => x.RequisitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CustomFieldDefinition)
            .WithMany()
            .HasForeignKey(x => x.CustomFieldDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### `JobPostingCustomFieldConfiguration.cs` [NEW]
```csharp
using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class JobPostingCustomFieldConfiguration : IEntityTypeConfiguration<JobPostingCustomField>
{
    public void Configure(EntityTypeBuilder<JobPostingCustomField> builder)
    {
        builder.ToTable("JobPostingCustomFields");
        builder.HasKey(x => new { x.JobPostingId, x.CustomFieldDefinitionId });

        builder.HasOne(x => x.JobPosting)
            .WithMany()
            .HasForeignKey(x => x.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CustomFieldDefinition)
            .WithMany()
            .HasForeignKey(x => x.CustomFieldDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## Core Persistence Update (`CustomFieldPersistence.cs`)

When querying custom fields for validation and mapping, we must dynamically merge **Global** definitions with **Instance-Specific** definitions linked via the join tables.

### For Requisition:
```csharp
var defs = await db.CustomFieldDefinitions
    .Where(d => d.TargetEntity == CustomFieldTargetEntity.Requisition && !d.IsDisabled &&
                (d.IsGlobal || db.RequisitionCustomFields.Any(rcf => rcf.RequisitionId == entityId && rcf.CustomFieldDefinitionId == d.Id)))
    .OrderBy(d => d.Order)
    .ToListAsync(ct);
```

### For Job Posting:
```csharp
var defs = await db.CustomFieldDefinitions
    .Where(d => d.TargetEntity == CustomFieldTargetEntity.JobPosting && !d.IsDisabled &&
                (d.IsGlobal || db.JobPostingCustomFields.Any(jpcf => jpcf.JobPostingId == entityId && jpcf.CustomFieldDefinitionId == d.Id)))
    .OrderBy(d => d.Order)
    .ToListAsync(ct);
```

### For Application:
To validate/fetch Application custom fields, we must traverse the hierarchy `Application -> JobPosting -> Requisition` to fetch linked definitions:
```csharp
var appInfo = await db.Applications
    .Where(a => a.Id == applicationId)
    .Select(a => new { a.JobPostingId, RequisitionId = a.JobPosting.RequisitionId })
    .FirstOrDefaultAsync(ct);

var scopes = new List<CustomFieldTargetEntity> { CustomFieldTargetEntity.Application_Global };
scopes.Add(CustomFieldTargetEntity.Application_Applied);
if (status == ApplicationStatus.Interview || status == ApplicationStatus.Offer || status == ApplicationStatus.Archive)
    scopes.Add(CustomFieldTargetEntity.Application_Interview);
if (status == ApplicationStatus.Offer || status == ApplicationStatus.Archive)
    scopes.Add(CustomFieldTargetEntity.Application_Offer);

var defs = await db.CustomFieldDefinitions
    .Where(d => scopes.Contains(d.TargetEntity) && !d.IsDisabled &&
                (d.IsGlobal || 
                 (appInfo != null && (
                     db.JobPostingCustomFields.Any(jpcf => jpcf.JobPostingId == appInfo.JobPostingId && jpcf.CustomFieldDefinitionId == d.Id) ||
                     db.RequisitionCustomFields.Any(rcf => rcf.RequisitionId == appInfo.RequisitionId && rcf.CustomFieldDefinitionId == d.Id)
                 ))))
    .OrderBy(d => d.TargetEntity)
    .ThenBy(d => d.Order)
    .ToListAsync(ct);
```

---

## API Contracts (`CustomFieldContracts.cs`)

### Existing Contracts Update
Add `IsGlobal` boolean parameter:
```csharp
record CreateCustomFieldRequest(
    CustomFieldTargetEntity TargetEntity,
    CustomFieldType FieldType,
    string Label,
    string? HelpText,
    bool IsRequired,
    bool IsCandidateFacing,
    ValidationOptionsDto? Validation,
    bool IsGlobal = true
);

record CustomFieldDefinitionDto(
    Guid Id,
    CustomFieldTargetEntity TargetEntity,
    CustomFieldType FieldType,
    string Label,
    string? HelpText,
    int Order,
    bool IsRequired,
    bool IsDisabled,
    bool IsCandidateFacing,
    ValidationOptionsDto? Validation,
    int ValueCount,
    bool IsGlobal,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
```

### New Linking Contracts
```csharp
record LinkCustomFieldRequest(Guid CustomFieldDefinitionId);
```

---

## Endpoints

### Central Settings
*   `GET /api/custom-fields/selectable`
    *   Query parameter: `targetEntity` (optional `CustomFieldTargetEntity`)
    *   Returns list of `CustomFieldDefinitionDto` where `IsGlobal == false` and matches target entity if specified. Ordered by `Label`.

### Requisition Links
*   `POST /api/requisitions/{id}/custom-fields`
    *   Adds entry to `RequisitionCustomFields`.
    *   Validation: Verify definition exists, is `IsGlobal == false`, target entity matches `Requisition`.
*   `DELETE /api/requisitions/{id}/custom-fields/{definitionId}`
    *   Checks if any values exist for this field on this requisition: `db.CustomFieldValues.AnyAsync(v => v.EntityId == id && v.CustomFieldDefinitionId == definitionId)`.
    *   If true → returns `409 Conflict` (cannot unlink field with collected data).
    *   If false → removes entry from `RequisitionCustomFields` and returns `204 No Content`.

### Job Posting Links
*   `POST /api/job-postings/{id}/custom-fields`
    *   Adds entry to `JobPostingCustomFields`.
    *   Validation: Verify definition exists, is `IsGlobal == false`, target entity matches `JobPosting` OR is an application scope (`Application_Global` or stage-specific).
*   `DELETE /api/job-postings/{id}/custom-fields/{definitionId}`
    *   Checks if any values exist for this field on this job posting or associated applications:
        *   If target is `JobPosting`: check values where `EntityId == id`.
        *   If target is `Application_*`: check values where `CustomFieldDefinitionId == definitionId` and `EntityId` is in the list of applications for this job posting.
    *   If true → returns `409 Conflict`.
    *   If false → removes entry from `JobPostingCustomFields` and returns `204 No Content`.
