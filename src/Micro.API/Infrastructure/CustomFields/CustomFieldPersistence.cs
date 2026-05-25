using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Endpoints.CustomFields;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Http;

namespace Micro.API.Infrastructure.CustomFields;

public class CustomFieldFilter
{
    public Guid DefinitionId { get; set; }
    public string? Value { get; set; }
    public string? Min { get; set; }
    public string? Max { get; set; }
}

public static class CustomFieldPersistence
{
    public static Dictionary<Guid, CustomFieldFilter> ParseFilters(IQueryCollection query)
    {
        var cfFilters = new Dictionary<Guid, CustomFieldFilter>();
        foreach (var key in query.Keys)
        {
            if (key.StartsWith("cfFilter["))
            {
                var rest = key["cfFilter[".Length..];
                var bracketIndex = rest.IndexOf(']');
                if (bracketIndex > 0)
                {
                    var idStr = rest[..bracketIndex];
                    if (Guid.TryParse(idStr, out var defId))
                    {
                        if (!cfFilters.TryGetValue(defId, out var filter))
                        {
                            filter = new CustomFieldFilter { DefinitionId = defId };
                            cfFilters[defId] = filter;
                        }

                        var suffix = rest[(bracketIndex + 1)..];
                        var queryVal = query[key].ToString();
                        if (suffix == "[min]")
                        {
                            filter.Min = queryVal;
                        }
                        else if (suffix == "[max]")
                        {
                            filter.Max = queryVal;
                        }
                        else if (string.IsNullOrEmpty(suffix))
                        {
                            filter.Value = queryVal;
                        }
                    }
                }
            }
        }
        return cfFilters;
    }
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

        var submittedList = submitted ?? Enumerable.Empty<CustomFieldValueInput>();

        foreach (var input in submittedList.Where(i => i.Value is not null))
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

        // Handle values set to null or empty: if not required, it can be deleted or set empty.
        // Wait, does the spec say we delete empty values? Yes, or just save them as empty. Let's delete empty values if they exist, or update them.
        // Let's delete them to clean up the DB:
        foreach (var input in submittedList.Where(i => i.Value is null or ""))
        {
            var row = existing.FirstOrDefault(e => e.CustomFieldDefinitionId == input.DefinitionId);
            if (row is not null)
            {
                db.CustomFieldValues.Remove(row);
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

    /// <summary>
    /// Returns custom field values for an application based on its current status, spanning multiple scopes.
    /// </summary>
    public static async Task<List<CustomFieldValueDto>> GetApplicationValuesAsync(
        MicroDbContext db,
        Guid applicationId,
        ApplicationStatus status,
        CancellationToken ct = default)
    {
        var scopes = new List<CustomFieldTargetEntity> { CustomFieldTargetEntity.Application_Global };
        
        scopes.Add(CustomFieldTargetEntity.Application_Applied);
        
        if (status == ApplicationStatus.Interview || status == ApplicationStatus.Offer || status == ApplicationStatus.Archive)
            scopes.Add(CustomFieldTargetEntity.Application_Interview);
            
        if (status == ApplicationStatus.Offer || status == ApplicationStatus.Archive)
            scopes.Add(CustomFieldTargetEntity.Application_Offer);

        var query = db.CustomFieldValues
            .Include(v => v.Definition)
            .Where(v => v.EntityId == applicationId && scopes.Contains(v.TargetEntity));

        return await query
            .OrderBy(v => v.Definition.TargetEntity)
            .ThenBy(v => v.Definition.Order)
            .Select(v => new CustomFieldValueDto(
                v.CustomFieldDefinitionId,
                v.Definition.Label,
                v.Definition.FieldType,
                v.Value,
                v.Definition.IsDisabled))
            .ToListAsync(ct);
    }

    public static async Task<Dictionary<string, string[]>?> ValidateApplicationAsync(
        MicroDbContext db,
        Guid applicationId,
        ApplicationStatus status,
        IEnumerable<CustomFieldValueInput> submitted,
        CancellationToken ct = default)
    {
        var errors = new Dictionary<string, string[]>();
        var scopes = new List<CustomFieldTargetEntity> { CustomFieldTargetEntity.Application_Global };
        scopes.Add(CustomFieldTargetEntity.Application_Applied);
        if (status == ApplicationStatus.Interview || status == ApplicationStatus.Offer || status == ApplicationStatus.Archive)
            scopes.Add(CustomFieldTargetEntity.Application_Interview);
        if (status == ApplicationStatus.Offer || status == ApplicationStatus.Archive)
            scopes.Add(CustomFieldTargetEntity.Application_Offer);

        foreach (var scope in scopes)
        {
            var cfErrors = await ValidateAsync(db, scope, applicationId, submitted, ct);
            if (cfErrors is not null)
            {
                foreach (var err in cfErrors)
                    errors[err.Key] = err.Value;
            }
        }

        return errors.Count > 0 ? errors : null;
    }

    public static async Task PersistApplicationAsync(
        MicroDbContext db,
        Guid applicationId,
        ApplicationStatus status,
        IEnumerable<CustomFieldValueInput> submitted,
        CancellationToken ct = default)
    {
        var scopes = new List<CustomFieldTargetEntity> { CustomFieldTargetEntity.Application_Global };
        scopes.Add(CustomFieldTargetEntity.Application_Applied);
        if (status == ApplicationStatus.Interview || status == ApplicationStatus.Offer || status == ApplicationStatus.Archive)
            scopes.Add(CustomFieldTargetEntity.Application_Interview);
        if (status == ApplicationStatus.Offer || status == ApplicationStatus.Archive)
            scopes.Add(CustomFieldTargetEntity.Application_Offer);

        foreach (var scope in scopes)
        {
            await PersistAsync(db, scope, applicationId, submitted, ct);
        }
    }

    public static async Task<Dictionary<string, string[]>?> ValidateCandidateFacingAsync(
        MicroDbContext db,
        Guid applicationId,
        IEnumerable<CustomFieldValueInput> submitted,
        CancellationToken ct = default)
    {
        var defs = await db.CustomFieldDefinitions
            .Where(d => (d.TargetEntity == CustomFieldTargetEntity.Application_Global || d.TargetEntity == CustomFieldTargetEntity.Application_Applied)
                        && d.IsCandidateFacing && !d.IsDisabled)
            .OrderBy(d => d.Order)
            .ToListAsync(ct);

        var submittedMap = submitted.ToDictionary(x => x.DefinitionId, x => x.Value);
        var errors = new Dictionary<string, string[]>();

        foreach (var def in defs)
        {
            submittedMap.TryGetValue(def.Id, out var value);
            var fieldErrors = CustomFieldValidator.Validate(def, value).ToArray();
            if (fieldErrors.Length > 0)
                errors[$"customFields.{def.Id}"] = fieldErrors;
        }

        return errors.Count > 0 ? errors : null;
    }

    public static async Task PersistCandidateFacingAsync(
        MicroDbContext db,
        Guid applicationId,
        IEnumerable<CustomFieldValueInput> submitted,
        CancellationToken ct = default)
    {
        var defs = await db.CustomFieldDefinitions
            .Where(d => (d.TargetEntity == CustomFieldTargetEntity.Application_Global || d.TargetEntity == CustomFieldTargetEntity.Application_Applied)
                        && d.IsCandidateFacing && !d.IsDisabled)
            .ToDictionaryAsync(d => d.Id, d => d.TargetEntity, ct);

        var submittedList = submitted ?? Enumerable.Empty<CustomFieldValueInput>();
        var grouped = submittedList
            .Where(x => defs.ContainsKey(x.DefinitionId))
            .GroupBy(x => defs[x.DefinitionId]);

        foreach (var group in grouped)
        {
            await PersistAsync(db, group.Key, applicationId, group, ct);
        }
    }
}
