# 021: API Choice Sync — Backend Design

## API Contracts & Configuration

### Application Settings (`appsettings.json`)
Configure the API key for sync authentication:
```json
{
  "CustomFields": {
    "ApiKey": "secure-sync-token-here"
  }
}
```

### Sync Endpoint Contracts
```csharp
public record SyncChoicesRequest(List<string> Choices);
```

---

## Endpoint Configuration


### Route Configuration (`CustomFieldEndpoints.cs`)
Map the new sync endpoint with an `.AllowAnonymous()` policy, bypassed from JWT validation.
```csharp
app.MapPut("/api/custom-fields/{id}/choices", SyncChoices)
   .AllowAnonymous();
```

### OpenAPI / Swagger Documentation
Document the `X-API-Key` header parameter for the sync endpoint to support UI-driven testing (Scalar/Swagger):
- Add custom parameter configuration metadata or configure Scalar to list the security scheme for the endpoint.

---

## Technical Design & Implementation Details

### API Key Verification
The endpoint reads the `X-API-Key` header from the request:
- If missing, or if it does not match `CustomFields:ApiKey` from configuration, return `401 Unauthorized`.

### Options Archiving and Sync Flow
1. Load `CustomFieldDefinition` from the database.
   - If not found, return `404 NotFound`.
   - If `FieldType` is not `CustomFieldType.SingleChoice`, return `400 BadRequest`.
2. Normalize inputs: Trim whitespace and filter out nulls/empty options from the incoming `Choices` list.
3. Fetch all `CustomFieldValue` entries for the field to check which options are in use.
4. Calculate new set of active `Choices` and `DisabledChoices`:
   - Any option in incoming choices list is set as active in `Choices`. (If it was in `DisabledChoices`, it is moved back).
   - Any option currently in the active `Choices` or `DisabledChoices` that is **not** in the incoming choices list is evaluated:
     - If it has associated `CustomFieldValue` records, it is added to `DisabledChoices`.
     - If it has no associated values, it is discarded entirely.
5. Serialize updated `ValidationOptions` back into `ValidationJson` and save changes.

### Manual Update Alignment
Update `UpdateCustomField` (in `CustomFieldEndpoints.cs`):
- Instead of returning a `409 Conflict` (lines 220-233) when `Choices` are removed and in use:
  - Apply the same archiving logic: move any removed choice that is in use to `DisabledChoices` and allow the update to succeed.
  - This ensures unified, predictable choice management across both manual settings edits and API-driven syncs.

### Model / Schema Updates


#### `ValidationOptions.cs` [Modified]
Add `DisabledChoices`:
```csharp
public record ValidationOptions
{
    // Existing fields...
    public List<string>? Choices { get; init; }
    public List<string>? DisabledChoices { get; init; }
}
```

#### `ValidationOptionsDto` in `CustomFieldContracts.cs` [Modified]
```csharp
public record ValidationOptionsDto(
    int? MinLength,
    int? MaxLength,
    decimal? Min,
    decimal? Max,
    DateOnly? MinDate,
    DateOnly? MaxDate,
    List<string>? Presets,
    string? FormatMask,
    List<string>? Choices,
    List<string>? DisabledChoices = null
);
```

#### Map DTO in `CustomFieldEndpoints.cs`
Update `SerializeValidation` and `MapToDto` to map `DisabledChoices`.

---

## Custom Field Validation Update

Update `CustomFieldValidator.Validate` signature to accept `existingValue`:
```csharp
public static IEnumerable<string> Validate(CustomFieldDefinition def, string? value, string? existingValue = null)
```


In `CustomFieldType.SingleChoice` validation block:
```csharp
case CustomFieldType.SingleChoice:
    var isActive = opts?.Choices is { Count: > 0 } && opts.Choices.Contains(value);
    var isExisting = existingValue == value;
    var isDisabled = opts?.DisabledChoices is { Count: > 0 } && opts.DisabledChoices.Contains(value);
    
    if (!isActive && !(isExisting && isDisabled))
        errors.Add($"'{def.Label}' contains an invalid value.");
    break;
```
Update calls in `CustomFieldPersistence.cs` to pass existing values from the database.

---

## Lookup Entity Synchronization (Departments, Salary Bands, Cost Centers)

### Additional Schema Modifications
To support consistent deactivation/retirement behavior across master data lookup tables, add `IsActive` to `SalaryBand` and `CostCenter`:

#### `SalaryBand.cs` [Modified]
```csharp
public class SalaryBand
{
    // Existing fields...
    public bool IsActive { get; set; } = true;
}
```

#### `CostCenter.cs` [Modified]
```csharp
public class CostCenter
{
    // Existing fields...
    public bool IsActive { get; set; } = true;
}
```

Add migration and apply it: `dotnet ef migrations add AddIsActiveToSalaryBandAndCostCenter` and update database.

### Sync Endpoints Route Mapping
Add three new endpoints to `AdminEndpoints.cs` (or specialized endpoint class) matching `X-API-Key` authentication:
- `PUT /api/departments/sync` -> Syncs a list of `Department` entries.
- `PUT /api/salary-bands/sync` -> Syncs a list of `SalaryBand` entries.
- `PUT /api/cost-centers/sync` -> Syncs a list of `CostCenter` entries.

These endpoints must accept `.AllowAnonymous()` and perform API Key header verification.

### Lookup Sync Execution Logic
1. Upsert loop:
   - For each item in the payload, query by its `Id` (and/or natural keys).
   - If present, update properties and set `IsActive = true`.
   - If not present, create a new record with `IsActive = true`.
2. Deactivate/Delete loop:
   - Identify database items not present in the payload.
   - For each item:
     - Check if it is referenced by any existing `Requisition` record.
     - If referenced: set `IsActive = false`.
     - If not referenced: delete from database.
3. Save changes in a single transaction.
