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
    public static async Task<List<CustomFieldDefinition>> GetActiveDefinitionsAsync(
        MicroDbContext db,
        CustomFieldTargetEntity targetEntity,
        Guid? entityId,
        Guid? requisitionId = null,
        Guid? jobPostingId = null,
        CancellationToken ct = default)
    {
        var query = db.CustomFieldDefinitions
            .Where(d => d.TargetEntity == targetEntity && !d.IsDisabled);

        if (targetEntity == CustomFieldTargetEntity.Requisition)
        {
            var reqId = entityId ?? requisitionId;
            if (reqId.HasValue && reqId != Guid.Empty)
            {
                query = query.Where(d => d.IsGlobal || db.RequisitionCustomFields.Any(rcf => rcf.RequisitionId == reqId && rcf.CustomFieldDefinitionId == d.Id));
            }
            else
            {
                query = query.Where(d => d.IsGlobal);
            }
        }
        else if (targetEntity == CustomFieldTargetEntity.JobPosting)
        {
            var jpId = entityId ?? jobPostingId;
            if (jpId.HasValue && jpId != Guid.Empty)
            {
                query = query.Where(d => d.IsGlobal || db.JobPostingCustomFields.Any(jpcf => jpcf.JobPostingId == jpId && jpcf.CustomFieldDefinitionId == d.Id));
            }
            else
            {
                query = query.Where(d => d.IsGlobal);
            }
        }
        else if (targetEntity == CustomFieldTargetEntity.Application_Global ||
                 targetEntity == CustomFieldTargetEntity.Application_Applied ||
                 targetEntity == CustomFieldTargetEntity.Application_Interview ||
                 targetEntity == CustomFieldTargetEntity.Application_Offer)
        {
            Guid? finalJobPostingId = jobPostingId;
            Guid? finalRequisitionId = requisitionId;

            if (entityId.HasValue && entityId != Guid.Empty)
            {
                var appInfo = await db.Applications
                    .Where(a => a.Id == entityId)
                    .Select(a => new { a.JobPostingId, RequisitionId = a.JobPosting.RequisitionId })
                    .FirstOrDefaultAsync(ct);

                if (appInfo != null)
                {
                    finalJobPostingId = appInfo.JobPostingId;
                    finalRequisitionId = appInfo.RequisitionId;
                }
            }

            if (finalJobPostingId.HasValue || finalRequisitionId.HasValue)
            {
                query = query.Where(d => d.IsGlobal ||
                                         (finalJobPostingId.HasValue && db.JobPostingCustomFields.Any(jpcf => jpcf.JobPostingId == finalJobPostingId && jpcf.CustomFieldDefinitionId == d.Id)) ||
                                         (finalRequisitionId.HasValue && db.RequisitionCustomFields.Any(rcf => rcf.RequisitionId == finalRequisitionId && rcf.CustomFieldDefinitionId == d.Id)));
            }
            else
            {
                query = query.Where(d => d.IsGlobal);
            }
        }

        return await query.OrderBy(d => d.Order).ToListAsync(ct);
    }

    public static async Task<Dictionary<string, string[]>?> ValidateAsync(
        MicroDbContext db,
        CustomFieldTargetEntity targetEntity,
        Guid entityId,
        IEnumerable<CustomFieldValueInput> submitted,
        CancellationToken ct = default)
    {
        var defs = await GetActiveDefinitionsAsync(db, targetEntity, entityId, null, null, ct);

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

            stored.TryGetValue(def.Id, out var existingVal);

            var fieldErrors = CustomFieldValidator.Validate(def, value, existingVal).ToArray();
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

        // Fetch application-linked definitions (global + linked to job posting or requisition)
        var appInfo = await db.Applications
            .Where(a => a.Id == applicationId)
            .Select(a => new { a.JobPostingId, RequisitionId = a.JobPosting.RequisitionId })
            .FirstOrDefaultAsync(ct);

        var query = db.CustomFieldValues
            .Include(v => v.Definition)
            .Where(v => v.EntityId == applicationId && scopes.Contains(v.TargetEntity));

        // If not global, verify association exists
        if (appInfo != null)
        {
            query = query.Where(v => v.Definition.IsGlobal ||
                                     db.JobPostingCustomFields.Any(jpcf => jpcf.JobPostingId == appInfo.JobPostingId && jpcf.CustomFieldDefinitionId == v.CustomFieldDefinitionId) ||
                                     db.RequisitionCustomFields.Any(rcf => rcf.RequisitionId == appInfo.RequisitionId && rcf.CustomFieldDefinitionId == v.CustomFieldDefinitionId));
        }
        else
        {
            query = query.Where(v => v.Definition.IsGlobal);
        }

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
        Guid jobPostingId,
        Guid applicationId,
        IEnumerable<CustomFieldValueInput> submitted,
        CancellationToken ct = default)
    {
        var globalDefs = await GetActiveDefinitionsAsync(db, CustomFieldTargetEntity.Application_Global, applicationId, null, jobPostingId, ct);
        var appliedDefs = await GetActiveDefinitionsAsync(db, CustomFieldTargetEntity.Application_Applied, applicationId, null, jobPostingId, ct);

        var defs = globalDefs.Concat(appliedDefs)
            .Where(d => d.IsCandidateFacing && !d.IsDisabled)
            .OrderBy(d => d.Order)
            .ToList();

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
        Guid jobPostingId,
        Guid applicationId,
        IEnumerable<CustomFieldValueInput> submitted,
        CancellationToken ct = default)
    {
        var globalDefs = await GetActiveDefinitionsAsync(db, CustomFieldTargetEntity.Application_Global, applicationId, null, jobPostingId, ct);
        var appliedDefs = await GetActiveDefinitionsAsync(db, CustomFieldTargetEntity.Application_Applied, applicationId, null, jobPostingId, ct);

        var defs = globalDefs.Concat(appliedDefs)
            .Where(d => d.IsCandidateFacing && !d.IsDisabled)
            .ToDictionary(d => d.Id, d => d.TargetEntity);

        var submittedList = submitted ?? Enumerable.Empty<CustomFieldValueInput>();
        var grouped = submittedList
            .Where(x => defs.ContainsKey(x.DefinitionId))
            .GroupBy(x => defs[x.DefinitionId]);

        foreach (var group in grouped)
        {
            await PersistAsync(db, group.Key, applicationId, group, ct);
        }
    }

    public static async Task<Dictionary<Guid, List<CustomFieldValueDto>>> GetBatchApplicationValuesAsync(
        MicroDbContext db,
        IEnumerable<Guid> applicationIds,
        CancellationToken ct = default)
    {
        var appIdsList = applicationIds.Distinct().ToList();
        if (appIdsList.Count == 0) return new();

        var appsInfo = await db.Applications
            .AsNoTracking()
            .Where(a => appIdsList.Contains(a.Id))
            .Select(a => new { a.Id, a.Status, a.JobPostingId, RequisitionId = a.JobPosting.RequisitionId })
            .ToListAsync(ct);

        var appsInfoMap = appsInfo.ToDictionary(a => a.Id);

        var allValues = await db.CustomFieldValues
            .AsNoTracking()
            .Include(v => v.Definition)
            .Where(v => appIdsList.Contains(v.EntityId))
            .ToListAsync(ct);

        var valuesByApp = allValues
            .GroupBy(v => v.EntityId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var jobPostingIds = appsInfo.Select(a => a.JobPostingId).Distinct().ToList();
        var requisitionIds = appsInfo.Select(a => a.RequisitionId).Distinct().ToList();

        var jobPostingFields = await db.JobPostingCustomFields
            .AsNoTracking()
            .Where(jpcf => jobPostingIds.Contains(jpcf.JobPostingId))
            .Select(jpcf => new { jpcf.JobPostingId, jpcf.CustomFieldDefinitionId })
            .ToListAsync(ct);

        var jobPostingFieldsMap = jobPostingFields
            .GroupBy(x => x.JobPostingId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.CustomFieldDefinitionId).ToHashSet());

        var requisitionFields = await db.RequisitionCustomFields
            .AsNoTracking()
            .Where(rcf => requisitionIds.Contains(rcf.RequisitionId))
            .Select(rcf => new { rcf.RequisitionId, rcf.CustomFieldDefinitionId })
            .ToListAsync(ct);

        var requisitionFieldsMap = requisitionFields
            .GroupBy(x => x.RequisitionId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.CustomFieldDefinitionId).ToHashSet());

        var result = new Dictionary<Guid, List<CustomFieldValueDto>>();

        foreach (var appId in appIdsList)
        {
            if (!appsInfoMap.TryGetValue(appId, out var app))
            {
                result[appId] = new();
                continue;
            }

            var allowedScopes = new HashSet<CustomFieldTargetEntity> { CustomFieldTargetEntity.Application_Global, CustomFieldTargetEntity.Application_Applied };
            if (app.Status is ApplicationStatus.Interview or ApplicationStatus.Offer or ApplicationStatus.Archive)
            {
                allowedScopes.Add(CustomFieldTargetEntity.Application_Interview);
            }
            if (app.Status is ApplicationStatus.Offer or ApplicationStatus.Archive)
            {
                allowedScopes.Add(CustomFieldTargetEntity.Application_Offer);
            }

            var appValues = valuesByApp.TryGetValue(appId, out var vals) ? vals : new List<CustomFieldValue>();

            var filtered = appValues
                .Where(v => allowedScopes.Contains(v.TargetEntity))
                .Where(v =>
                {
                    if (v.Definition.IsGlobal) return true;

                    var isLinkedToJp = jobPostingFieldsMap.TryGetValue(app.JobPostingId, out var jpSet) && jpSet.Contains(v.CustomFieldDefinitionId);
                    var isLinkedToReq = requisitionFieldsMap.TryGetValue(app.RequisitionId, out var reqSet) && reqSet.Contains(v.CustomFieldDefinitionId);

                    return isLinkedToJp || isLinkedToReq;
                })
                .OrderBy(v => v.Definition.TargetEntity)
                .ThenBy(v => v.Definition.Order)
                .Select(v => new CustomFieldValueDto(
                    v.CustomFieldDefinitionId,
                    v.Definition.Label,
                    v.Definition.FieldType,
                    v.Value,
                    v.Definition.IsDisabled))
                .ToList();

            result[appId] = filtered;
        }

        return result;
    }

    public static async Task<Dictionary<Guid, List<CustomFieldValueDto>>> GetBatchRequisitionValuesAsync(
        MicroDbContext db,
        IEnumerable<Guid> requisitionIds,
        CancellationToken ct = default)
    {
        var reqIdsList = requisitionIds.Distinct().ToList();
        if (reqIdsList.Count == 0) return new();

        var allValues = await db.CustomFieldValues
            .AsNoTracking()
            .Include(v => v.Definition)
            .Where(v => reqIdsList.Contains(v.EntityId) && v.TargetEntity == CustomFieldTargetEntity.Requisition)
            .ToListAsync(ct);

        var valuesByReq = allValues
            .GroupBy(v => v.EntityId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(v => v.Definition.Order)
                      .Select(v => new CustomFieldValueDto(
                          v.CustomFieldDefinitionId,
                          v.Definition.Label,
                          v.Definition.FieldType,
                          v.Value,
                          v.Definition.IsDisabled))
                      .ToList()
            );

        var result = new Dictionary<Guid, List<CustomFieldValueDto>>();
        foreach (var reqId in reqIdsList)
        {
            result[reqId] = valuesByReq.TryGetValue(reqId, out var list) ? list : new();
        }

        return result;
    }
}
