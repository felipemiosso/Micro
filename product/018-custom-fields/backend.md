# 018: Custom Fields — Backend Design

## New Files

```
src/Micro.API/
  Data/
    Models/
      CustomFieldDefinition.cs     [NEW]
      CustomFieldValue.cs          [NEW]
    Configuration/
      CustomFieldDefinitionConfiguration.cs  [NEW]
      CustomFieldValueConfiguration.cs       [NEW]
  Infrastructure/
    CustomFields/
      ValidatorRegistry.cs         [NEW]
      CustomFieldValidator.cs      [NEW]
      MaskValidator.cs             [NEW]
      Presets/
        CpfValidator.cs            [NEW]
        CnpjValidator.cs           [NEW]
        PisValidator.cs            [NEW]
        CnhValidator.cs            [NEW]
        PhoneMobileValidator.cs    [NEW]
        PhoneLandlineValidator.cs  [NEW]
        CepValidator.cs            [NEW]
  Endpoints/
    CustomFields/
      CustomFieldEndpoints.cs      [NEW]
      CustomFieldContracts.cs      [NEW]
```

## Modified Files

```
src/Micro.API/
  Data/
    MicroDbContext.cs
  Endpoints/
    Requisition/
      RequisitionContracts.cs
      RequisitionEndpoints.cs
    Application/
      ApplicationContracts.cs
      ApplicationEndpoints.cs
    JobPosting/
      JobPostingContracts.cs
      JobPostingEndpoints.cs       ← public apply + admin views
    Candidate/
      CandidateContracts.cs
```

---

## Domain Models

### `CustomFieldDefinition.cs`

```csharp
namespace Micro.API.Data.Models;

public class CustomFieldDefinition
{
    public Guid Id { get; set; }
    public CustomFieldTargetEntity TargetEntity { get; set; }
    public CustomFieldType FieldType { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? HelpText { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsCandidateFacing { get; set; }
    public string? ValidationJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<CustomFieldValue> Values { get; set; } = new List<CustomFieldValue>();
}

public enum CustomFieldTargetEntity
{
    Requisition,
    Application_Global,
    Application_Applied,
    Application_Interview,
    Application_Offer,
    JobPosting
}

public enum CustomFieldType
{
    ShortText,
    LongText,
    Number,
    Date,
    Boolean,
    SingleChoice
}
```

### `CustomFieldValue.cs`

```csharp
namespace Micro.API.Data.Models;

public class CustomFieldValue
{
    public Guid Id { get; set; }
    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition Definition { get; set; } = null!;
    public Guid EntityId { get; set; }
    public CustomFieldTargetEntity TargetEntity { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

### `ValidationOptions.cs` (deserialization record, not an EF entity)

Place in `Data/Models/`:

```csharp
namespace Micro.API.Data.Models;

public record ValidationOptions
{
    public int? MinLength { get; init; }
    public int? MaxLength { get; init; }
    public decimal? Min { get; init; }
    public decimal? Max { get; init; }
    public DateOnly? MinDate { get; init; }
    public DateOnly? MaxDate { get; init; }
    // OR logic: value must satisfy at least one preset
    public List<string>? Presets { get; init; }
    public string? FormatMask { get; init; }
    public List<string>? Choices { get; init; }
}
```

---

## EF Configuration

### `CustomFieldDefinitionConfiguration.cs`

```csharp
public class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CustomFieldDefinition> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label).IsRequired().HasMaxLength(200);
        builder.Property(x => x.HelpText).HasMaxLength(500);
        builder.Property(x => x.ValidationJson).HasColumnType("jsonb");
        builder.Property(x => x.TargetEntity).HasConversion<string>();
        builder.Property(x => x.FieldType).HasConversion<string>();

        // Queries always filter by entity + active status + order
        builder.HasIndex(x => new { x.TargetEntity, x.IsDisabled, x.Order });
    }
}
```

### `CustomFieldValueConfiguration.cs`

```csharp
public class CustomFieldValueConfiguration : IEntityTypeConfiguration<CustomFieldValue>
{
    public void Configure(EntityTypeBuilder<CustomFieldValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(10000);
        builder.Property(x => x.TargetEntity).HasConversion<string>();

        // Primary lookup pattern: all values for a given entity record
        builder.HasIndex(x => new { x.EntityId, x.TargetEntity });
        // Secondary: all values for a given field definition (count queries, rule-change pre-flight)
        builder.HasIndex(x => x.CustomFieldDefinitionId);

        builder.HasOne(x => x.Definition)
            .WithMany(x => x.Values)
            .HasForeignKey(x => x.CustomFieldDefinitionId)
            .OnDelete(DeleteBehavior.Restrict); // never cascade-delete values
    }
}
```

### `MicroDbContext.cs` additions

```csharp
public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();
```

---

## Infrastructure — Validator Registry

### `ValidatorRegistry.cs`

```csharp
namespace Micro.API.Infrastructure.CustomFields;

public static class ValidatorRegistry
{
    public record PresetDefinition(string Label, string Tag, Func<string, ValidationResult> Validate);
    public record ValidationResult(bool IsValid, string? ErrorMessage = null);

    public static readonly IReadOnlyDictionary<string, PresetDefinition> Presets =
        new Dictionary<string, PresetDefinition>
        {
            ["cpf"]            = new("CPF",               "Documentos",  CpfValidator.Validate),
            ["cnpj"]           = new("CNPJ",              "Documentos",  CnpjValidator.Validate),
            ["pis"]            = new("PIS / PASEP",        "Documentos",  PisValidator.Validate),
            ["cnh"]            = new("CNH",               "Documentos",  CnhValidator.Validate),
            ["email"]          = new("E-mail",            "Contato",     EmailValidator.Validate),
            ["phone_mobile"]   = new("Telefone (celular)","Contato",     PhoneMobileValidator.Validate),
            ["phone_landline"] = new("Telefone (fixo)",   "Contato",     PhoneLandlineValidator.Validate),
            ["url"]            = new("URL",               "Contato",     UrlValidator.Validate),
            ["cep"]            = new("CEP",               "Localização", CepValidator.Validate),
        };

    // Returns true if value satisfies at least one of the given preset keys (OR logic)
    public static ValidationResult ValidateAny(IEnumerable<string> presetKeys, string value)
    {
        var results = presetKeys
            .Where(Presets.ContainsKey)
            .Select(k => Presets[k].Validate(value))
            .ToList();

        if (results.Count == 0) return new(true);
        if (results.Any(r => r.IsValid)) return new(true);

        // All presets failed — return the first error message
        return results.First(r => !r.IsValid);
    }
}
```

Each preset class (e.g. `CpfValidator.cs`) lives in `Infrastructure/CustomFields/Presets/` and exposes a single static `Validate(string value) → ValidationResult` method implementing the relevant format + semantic checks.

### `MaskValidator.cs`

```csharp
namespace Micro.API.Infrastructure.CustomFields;

public static class MaskValidator
{
    // # = digit, A = letter, X = alphanumeric, else literal
    public static bool Matches(string mask, string value)
    {
        if (mask.Length != value.Length) return false;
        return mask.Zip(value).All(pair =>
            pair.First switch
            {
                '#' => char.IsDigit(pair.Second),
                'A' => char.IsLetter(pair.Second),
                'X' => char.IsLetterOrDigit(pair.Second),
                _   => pair.First == pair.Second
            });
    }
}
```

### `CustomFieldValidator.cs`

The central validation function used by every entity endpoint.

```csharp
namespace Micro.API.Infrastructure.CustomFields;

public static class CustomFieldValidator
{
    /// <summary>
    /// Validates a single submitted value against its field definition.
    /// Returns zero or more user-facing error strings.
    /// </summary>
    public static IEnumerable<string> Validate(CustomFieldDefinition def, string? value)
    {
        var errors = new List<string>();
        var opts = DeserializeOptions(def.ValidationJson);

        if (def.IsRequired && string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"'{def.Label}' é obrigatório.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(value)) return errors;

        switch (def.FieldType)
        {
            case CustomFieldType.ShortText:
            case CustomFieldType.LongText:
                if (opts?.MinLength.HasValue == true && value.Length < opts.MinLength)
                    errors.Add($"'{def.Label}' deve ter no mínimo {opts.MinLength} caracteres.");
                if (opts?.MaxLength.HasValue == true && value.Length > opts.MaxLength)
                    errors.Add($"'{def.Label}' deve ter no máximo {opts.MaxLength} caracteres.");

                if (def.FieldType == CustomFieldType.ShortText)
                {
                    if (opts?.Presets is { Count: > 0 })
                    {
                        var result = ValidatorRegistry.ValidateAny(opts.Presets, value);
                        if (!result.IsValid)
                            errors.Add($"'{def.Label}': {result.ErrorMessage ?? "formato inválido"}.");
                    }
                    else if (!string.IsNullOrEmpty(opts?.FormatMask))
                    {
                        if (!MaskValidator.Matches(opts.FormatMask, value))
                            errors.Add($"'{def.Label}' não corresponde ao formato esperado ({opts.FormatMask}).");
                    }
                }
                break;

            case CustomFieldType.Number:
                if (!decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var num))
                {
                    errors.Add($"'{def.Label}' deve ser um número válido.");
                    break;
                }
                if (opts?.Min.HasValue == true && num < opts.Min)
                    errors.Add($"'{def.Label}' deve ser maior ou igual a {opts.Min}.");
                if (opts?.Max.HasValue == true && num > opts.Max)
                    errors.Add($"'{def.Label}' deve ser menor ou igual a {opts.Max}.");
                break;

            case CustomFieldType.Date:
                if (!DateOnly.TryParse(value, out var date))
                {
                    errors.Add($"'{def.Label}' deve ser uma data válida.");
                    break;
                }
                if (opts?.MinDate.HasValue == true && date < opts.MinDate)
                    errors.Add($"'{def.Label}' deve ser a partir de {opts.MinDate:yyyy-MM-dd}.");
                if (opts?.MaxDate.HasValue == true && date > opts.MaxDate)
                    errors.Add($"'{def.Label}' deve ser até {opts.MaxDate:yyyy-MM-dd}.");
                break;

            case CustomFieldType.Boolean:
                if (value is not ("true" or "false"))
                    errors.Add($"'{def.Label}' deve ser verdadeiro ou falso.");
                break;

            case CustomFieldType.SingleChoice:
                if (opts?.Choices is { Count: > 0 } && !opts.Choices.Contains(value))
                    errors.Add($"'{def.Label}' contém um valor inválido.");
                break;
        }

        return errors;
    }

    private static ValidationOptions? DeserializeOptions(string? json) =>
        string.IsNullOrEmpty(json)
            ? null
            : System.Text.Json.JsonSerializer.Deserialize<ValidationOptions>(json);
}
```

---

## API Contracts

### `CustomFieldContracts.cs`

```csharp
namespace Micro.API.Endpoints.CustomFields;

// ── Requests ──────────────────────────────────────────────────────────────────

record CreateCustomFieldRequest(
    CustomFieldTargetEntity TargetEntity,
    CustomFieldType FieldType,
    string Label,
    string? HelpText,
    bool IsRequired,
    bool IsCandidateFacing,
    ValidationOptionsDto? Validation
);

record UpdateCustomFieldRequest(
    string Label,
    string? HelpText,
    bool IsRequired,
    bool IsCandidateFacing,
    ValidationOptionsDto? Validation
);

record ReorderCustomFieldsRequest(
    List<Guid> OrderedIds  // full ordered list for the entity/scope
);

record ValidationOptionsDto(
    int? MinLength,
    int? MaxLength,
    decimal? Min,
    decimal? Max,
    DateOnly? MinDate,
    DateOnly? MaxDate,
    List<string>? Presets,      // OR logic — multiple preset keys
    string? FormatMask,
    List<string>? Choices
);

// Used by all entity create/update endpoints to submit custom field values
record CustomFieldValueInput(
    Guid DefinitionId,
    string? Value               // null or empty = field not filled (only valid if not Required)
);

// ── Responses ─────────────────────────────────────────────────────────────────

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
    int ValueCount,             // always returned; used by admin UI for badge + delete decision
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// Embedded in entity detail/list responses
record CustomFieldValueDto(
    Guid DefinitionId,
    string Label,
    CustomFieldType FieldType,
    string? Value,
    bool IsDisabled             // true = show "Campo desativado" badge
);

// Returned by /available-presets
record PresetGroupDto(
    string Tag,
    List<PresetDto> Presets
);

record PresetDto(string Key, string Label);
```

---

## Endpoints — `CustomFieldEndpoints.cs`

All management routes require the `Admin` policy.

```
GET    /api/custom-fields
GET    /api/custom-fields/available-presets
GET    /api/custom-fields/{id}
POST   /api/custom-fields
PUT    /api/custom-fields/{id}
PATCH  /api/custom-fields/{id}/disable
PATCH  /api/custom-fields/{id}/enable
PUT    /api/custom-fields/reorder
DELETE /api/custom-fields/{id}
```

### `GET /api/custom-fields`

Query parameters:
- `entity` (required) — `CustomFieldTargetEntity` enum value
- `includeDisabled` (bool, default `false`) — when `true` also returns disabled fields
- `activeOnly` (bool, default `false`) — alias for `includeDisabled=false`; both params coexist for clarity

Returns `List<CustomFieldDefinitionDto>` ordered by `Order` ascending. `ValueCount` is always included.

### `GET /api/custom-fields/available-presets`

Returns `List<PresetGroupDto>` built from `ValidatorRegistry.Presets`, grouped by `Tag`. This is the canonical source the Angular admin UI uses to populate the preset selector — no frontend hardcoding of preset names.

### `POST /api/custom-fields`

Validation before saving:
1. `Label` must be non-empty (max 200 chars).
2. `TargetEntity` must be a valid enum value.
3. `FieldType` must be a valid enum value.
4. `IsCandidateFacing = true` is only valid when `TargetEntity` is `Application_Global` or `Application_Applied`.
5. If `FieldType` is `SingleChoice`, `Validation.Choices` must be non-null and non-empty.
6. If `Validation.FormatMask` is set, `FieldType` must be `ShortText` and `Validation.Presets` must be null/empty.
7. `Validation.Presets` keys must all exist in `ValidatorRegistry.Presets`.
8. `Order` is set server-side to `MAX(Order) + 1` for the target entity/scope group (client cannot specify it on create).

Returns `201 Created` with `CustomFieldDefinitionDto`.

### `PUT /api/custom-fields/{id}`

Performs **rule-change pre-flight checks** before saving. All checks query `CustomFieldValues` filtered by `CustomFieldDefinitionId = id`.

| Attempted change | Pre-flight check |
|---|---|
| `FieldType` changed | `AnyAsync(v => v.CustomFieldDefinitionId == id)` → 409 if count > 0 |
| `Presets` changed | same check |
| `FormatMask` changed | same check |
| `MaxLength` decreased or new max added | count values where `value.Length > newMax` → 409 if count > 0 |
| `MinLength` increased | count values where `value.Length < newMin` → 409 if count > 0 |
| `Choices` option removed | count values where `value == removedOption` → 409 if count > 0 |
| `IsCandidateFacing = true` | `TargetEntity` must be `Application_Global` or `Application_Applied` |

409 response body: `{ "code": "RULE_CHANGE_BLOCKED", "message": "...", "affectedCount": N }`

`Label`, `HelpText`, `IsRequired`, and `IsCandidateFacing` (within allowed scopes) can always be changed freely.

Returns `200 OK` with updated `CustomFieldDefinitionDto`.

### `PATCH /api/custom-fields/{id}/disable`

Sets `IsDisabled = true`. Does not check value count — disabling is always allowed. Returns `200 OK`.

### `PATCH /api/custom-fields/{id}/enable`

Sets `IsDisabled = false`. Returns `200 OK`.

### `PUT /api/custom-fields/reorder`

Accepts `ReorderCustomFieldsRequest` with a complete ordered list of IDs for a given entity/scope. Updates `Order` for each. All IDs must belong to the same `TargetEntity` group; otherwise returns `422`.

### `DELETE /api/custom-fields/{id}`

Pre-flight: `AnyAsync(v => v.CustomFieldDefinitionId == id)`.
- If true → `409 Conflict { "code": "FIELD_HAS_VALUES", "message": "..." }`
- If false → hard delete, `204 No Content`

---

## Custom Field Validation Helper for Entity Endpoints

Extract into `Infrastructure/CustomFields/CustomFieldPersistence.cs` to keep entity endpoints lean:

```csharp
public static class CustomFieldPersistence
{
    /// <summary>
    /// Validates all active custom field values for an entity.
    /// Validates ALL active fields — not just those submitted — enforcing retroactive rules.
    /// Returns a dictionary keyed by definitionId for use with Results.ValidationProblem().
    /// </summary>
    public static async Task<Dictionary<string, string[]>?> ValidateAsync(
        MicroDbContext db,
        CustomFieldTargetEntity targetEntity,
        Guid entityId,
        IEnumerable<CustomFieldValueInput> submitted,
        CancellationToken ct = default)
    {
        var defs = await db.CustomFieldDefinitions
            .Where(d => d.TargetEntity == targetEntity && !d.IsDisabled)
            .OrderBy(d => d.Order)
            .ToListAsync(ct);

        // Merge submitted values with already-stored values
        // Submitted values take precedence; stored values fill the rest
        var stored = await db.CustomFieldValues
            .Where(v => v.EntityId == entityId && v.TargetEntity == targetEntity)
            .ToDictionaryAsync(v => v.CustomFieldDefinitionId, v => v.Value, ct);

        var submittedMap = submitted.ToDictionary(x => x.DefinitionId, x => x.Value);

        var errors = new Dictionary<string, string[]>();
        foreach (var def in defs)
        {
            var value = submittedMap.TryGetValue(def.Id, out var s) ? s
                : stored.TryGetValue(def.Id, out var st) ? st
                : null;

            var fieldErrors = CustomFieldValidator.Validate(def, value).ToArray();
            if (fieldErrors.Length > 0)
                errors[$"customFields.{def.Id}"] = fieldErrors;
        }

        return errors.Count > 0 ? errors : null;
    }

    /// <summary>
    /// Upserts submitted custom field values. Only call after ValidateAsync passes.
    /// </summary>
    public static async Task PersistAsync(
        MicroDbContext db,
        CustomFieldTargetEntity targetEntity,
        Guid entityId,
        IEnumerable<CustomFieldValueInput> submitted,
        CancellationToken ct = default)
    {
        var existing = await db.CustomFieldValues
            .Where(v => v.EntityId == entityId && v.TargetEntity == targetEntity)
            .ToListAsync(ct);

        foreach (var input in submitted.Where(i => i.Value is not null))
        {
            var row = existing.FirstOrDefault(e => e.CustomFieldDefinitionId == input.DefinitionId);
            if (row is null)
            {
                db.CustomFieldValues.Add(new CustomFieldValue
                {
                    Id = Guid.NewGuid(),
                    CustomFieldDefinitionId = input.DefinitionId,
                    EntityId = entityId,
                    TargetEntity = targetEntity,
                    Value = input.Value!,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                row.Value = input.Value!;
                row.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Returns all custom field values for an entity, including disabled fields
    /// (for detail/profile views). Active-only flag for form rendering.
    /// </summary>
    public static async Task<List<CustomFieldValueDto>> GetValuesAsync(
        MicroDbContext db,
        CustomFieldTargetEntity targetEntity,
        Guid entityId,
        bool activeOnly = false,
        CancellationToken ct = default)
    {
        var query = db.CustomFieldValues
            .Include(v => v.Definition)
            .Where(v => v.EntityId == entityId && v.TargetEntity == targetEntity);

        if (activeOnly)
            query = query.Where(v => !v.Definition.IsDisabled);

        return await query
            .OrderBy(v => v.Definition.Order)
            .Select(v => new CustomFieldValueDto(
                v.CustomFieldDefinitionId,
                v.Definition.Label,
                v.Definition.FieldType,
                v.Value,
                v.Definition.IsDisabled))
            .ToListAsync(ct);
    }
}
```

---

## Changes to Existing Entity Endpoints

### Pattern for create / update endpoints (Requisition, JobPosting)

All create and update request records gain:
```csharp
List<CustomFieldValueInput>? CustomFieldValues
```

In the handler, after standard field validation:
```csharp
var cfErrors = await CustomFieldPersistence.ValidateAsync(
    db, CustomFieldTargetEntity.Requisition, entity.Id,
    request.CustomFieldValues ?? [], ct);

if (cfErrors is not null)
    return Results.ValidationProblem(cfErrors);

// ... save entity ...

await CustomFieldPersistence.PersistAsync(
    db, CustomFieldTargetEntity.Requisition, entity.Id,
    request.CustomFieldValues ?? [], ct);
```

All detail response records gain:
```csharp
List<CustomFieldValueDto> CustomFields
```

Populated via `CustomFieldPersistence.GetValuesAsync(db, targetEntity, entity.Id, activeOnly: false)`.

### Application endpoints

Application has multiple scopes. The handler determines the applicable scopes based on `ApplicationStatus` and fetches values for all relevant scopes:

| ApplicationStatus | Scopes to load for display |
|---|---|
| Applied | `Application_Global`, `Application_Applied` |
| Interview | `Application_Global`, `Application_Applied`, `Application_Interview` |
| Offer | `Application_Global`, `Application_Applied`, `Application_Interview`, `Application_Offer` |
| Archive | all four scopes |

For **write** (form submission), only the fields matching the current stage's scope + Global are validated and persisted in that request. Previously stored values from other scopes are still validated in the retroactive check via `CustomFieldPersistence.ValidateAsync` (which reads all stored values and merges them with submitted).

### Public application form — `POST /api/public/jobs/{id}/apply`

1. Load candidate-facing definitions for `Application_Global` and `Application_Applied` where `IsCandidateFacing = true`.
2. Add `List<CustomFieldValueInput>? CustomFieldValues` to `ApplyRequest`.
3. Run `CustomFieldPersistence.ValidateAsync` using only the candidate-facing definitions.
4. Persist via `CustomFieldPersistence.PersistAsync` after application is created.

The public `GET /api/public/jobs/{id}` response includes the candidate-facing field definitions (label, help text, field type, validation rules) so the Angular public form can render them dynamically.

---

## Filtering

Entity list endpoints (`GET /api/requisitions`, `GET /api/applications`) accept an optional filter parameter:

```
GET /api/requisitions?cfFilter[{definitionId}]={value}
GET /api/requisitions?cfFilter[{definitionId}][min]={value}&cfFilter[{definitionId}][max]={value}
```

The endpoint parses these into a list of `CustomFieldFilter` objects and applies them as EXISTS subqueries against `CustomFieldValues`:

```csharp
// For ShortText/LongText/SingleChoice/Boolean — exact or ILIKE
query = query.Where(r =>
    db.CustomFieldValues.Any(v =>
        v.EntityId == r.Id &&
        v.CustomFieldDefinitionId == filter.DefinitionId &&
        EF.Functions.ILike(v.Value, $"%{filter.Value}%")));

// For Number — cast and range
query = query.Where(r =>
    db.CustomFieldValues.Any(v =>
        v.EntityId == r.Id &&
        v.CustomFieldDefinitionId == filter.DefinitionId &&
        double.Parse(v.Value) >= filter.Min &&
        double.Parse(v.Value) <= filter.Max));
```

Use `EF.Functions.ILike` for case-insensitive text search. Number and date range filtering casts the stored string value in the query. Indexes on `(EntityId, TargetEntity)` make these EXISTS checks efficient.

---

## Validation Rules Summary

| Rule | Enforced where |
|---|---|
| Definition-level invariants (label, type, mask/preset exclusivity, scope for IsCandidateFacing) | `POST` and `PUT /api/custom-fields` |
| Rule-change pre-flight (type, preset, mask, length, choices) | `PUT /api/custom-fields/{id}` |
| Field value validation on entity save | `CustomFieldPersistence.ValidateAsync` called from every entity create/update handler |
| Hard-delete blocked when values exist | `DELETE /api/custom-fields/{id}` |
| Candidate-facing validation on public apply | `POST /api/public/jobs/{id}/apply` |
