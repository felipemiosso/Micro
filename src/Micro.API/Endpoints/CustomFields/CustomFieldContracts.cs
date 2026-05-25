using System;
using System.Collections.Generic;
using Micro.API.Data.Models;

namespace Micro.API.Endpoints.CustomFields;

// ── Requests ──────────────────────────────────────────────────────────────────

public record CreateCustomFieldRequest(
    CustomFieldTargetEntity TargetEntity,
    CustomFieldType FieldType,
    string Label,
    string? HelpText,
    bool IsRequired,
    bool IsCandidateFacing,
    ValidationOptionsDto? Validation
);

public record UpdateCustomFieldRequest(
    string Label,
    string? HelpText,
    bool IsRequired,
    bool IsCandidateFacing,
    ValidationOptionsDto? Validation
);

public record ReorderCustomFieldsRequest(
    List<Guid> OrderedIds  // full ordered list for the entity/scope
);

public record ValidationOptionsDto(
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
public record CustomFieldValueInput(
    Guid DefinitionId,
    string? Value               // null or empty = field not filled (only valid if not Required)
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record CustomFieldDefinitionDto(
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
public record CustomFieldValueDto(
    Guid DefinitionId,
    string Label,
    CustomFieldType FieldType,
    string? Value,
    bool IsDisabled             // true = show "Campo desativado" badge
);

// Returned by /available-presets
public record PresetGroupDto(
    string Tag,
    List<PresetDto> Presets
);

public record PresetDto(string Key, string Label);
