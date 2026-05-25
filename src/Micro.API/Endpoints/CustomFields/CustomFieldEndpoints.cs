using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.CustomFields;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints.CustomFields;

public static class CustomFieldEndpoints
{
    public static void MapCustomFieldEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/custom-fields");

        // Authenticated GET requests (for users to retrieve active fields for forms)
        group.MapGet("/", GetCustomFields).RequireAuthorization();
        group.MapGet("/available-presets", GetAvailablePresets).RequireAuthorization();

        // Admin-only management endpoints
        group.MapGet("/{id:guid}", GetCustomFieldById).RequireAuthorization("Admin");
        group.MapPost("/", CreateCustomField).RequireAuthorization("Admin");
        group.MapPut("/{id:guid}", UpdateCustomField).RequireAuthorization("Admin");
        group.MapPatch("/{id:guid}/disable", DisableCustomField).RequireAuthorization("Admin");
        group.MapPatch("/{id:guid}/enable", EnableCustomField).RequireAuthorization("Admin");
        group.MapPut("/reorder", ReorderCustomFields).RequireAuthorization("Admin");
        group.MapDelete("/{id:guid}", DeleteCustomField).RequireAuthorization("Admin");
    }

    private static async Task<IResult> GetCustomFields(
        CustomFieldTargetEntity entity,
        bool includeDisabled = false,
        bool activeOnly = false,
        MicroDbContext db = null!)
    {
        var activeFilter = activeOnly || !includeDisabled;

        var query = db.CustomFieldDefinitions.Where(d => d.TargetEntity == entity);
        if (activeFilter)
        {
            query = query.Where(d => !d.IsDisabled);
        }

        var definitions = await query
            .OrderBy(d => d.Order)
            .ToListAsync();

        var dtos = new List<CustomFieldDefinitionDto>();
        foreach (var def in definitions)
        {
            var valueCount = await db.CustomFieldValues.CountAsync(v => v.CustomFieldDefinitionId == def.Id);
            dtos.Add(MapToDto(def, valueCount));
        }

        return Results.Ok(dtos);
    }

    private static IResult GetAvailablePresets()
    {
        var groups = ValidatorRegistry.Presets
            .GroupBy(p => p.Value.Tag)
            .Select(g => new PresetGroupDto(
                g.Key,
                g.Select(p => new PresetDto(p.Key, p.Value.Label)).ToList()
            ))
            .ToList();

        return Results.Ok(groups);
    }

    private static async Task<IResult> GetCustomFieldById(Guid id, MicroDbContext db)
    {
        var def = await db.CustomFieldDefinitions.FindAsync(id);
        if (def is null) return Results.NotFound();

        var valueCount = await db.CustomFieldValues.CountAsync(v => v.CustomFieldDefinitionId == def.Id);
        return Results.Ok(MapToDto(def, valueCount));
    }

    private static async Task<IResult> CreateCustomField(CreateCustomFieldRequest request, MicroDbContext db)
    {
        var validationErrors = ValidateRequest(request.Label, request.HelpText, request.TargetEntity, request.FieldType, request.IsCandidateFacing, request.Validation);
        if (validationErrors.Count > 0)
            return Results.ValidationProblem(validationErrors);

        // Calculate max order for target entity
        var maxOrder = await db.CustomFieldDefinitions
            .Where(d => d.TargetEntity == request.TargetEntity)
            .Select(d => (int?)d.Order)
            .MaxAsync() ?? 0;

        var def = new CustomFieldDefinition
        {
            Id = Guid.NewGuid(),
            TargetEntity = request.TargetEntity,
            FieldType = request.FieldType,
            Label = request.Label,
            HelpText = request.HelpText,
            Order = maxOrder + 1,
            IsRequired = request.IsRequired,
            IsDisabled = false,
            IsCandidateFacing = request.IsCandidateFacing,
            ValidationJson = SerializeValidation(request.Validation),
            CreatedAt = DateTime.UtcNow
        };

        db.CustomFieldDefinitions.Add(def);
        await db.SaveChangesAsync();

        return Results.Created($"/api/custom-fields/{def.Id}", MapToDto(def, 0));
    }

    private static async Task<IResult> UpdateCustomField(Guid id, UpdateCustomFieldRequest request, MicroDbContext db)
    {
        var def = await db.CustomFieldDefinitions.FindAsync(id);
        if (def is null) return Results.NotFound();

        var validationErrors = ValidateRequest(request.Label, request.HelpText, def.TargetEntity, def.FieldType, request.IsCandidateFacing, request.Validation);
        if (validationErrors.Count > 0)
            return Results.ValidationProblem(validationErrors);

        // Pre-flight validation when there is existing data
        var hasValues = await db.CustomFieldValues.AnyAsync(v => v.CustomFieldDefinitionId == id);
        if (hasValues)
        {
            var oldOpts = string.IsNullOrEmpty(def.ValidationJson) ? null : JsonSerializer.Deserialize<ValidationOptions>(def.ValidationJson);
            
            // Check presets changed
            var oldPresets = oldOpts?.Presets ?? new List<string>();
            var newPresets = request.Validation?.Presets ?? new List<string>();
            if (!oldPresets.SequenceEqual(newPresets))
            {
                return Results.Conflict(new { code = "RULE_CHANGE_BLOCKED", message = "Cannot change presets because data exists.", affectedCount = await db.CustomFieldValues.CountAsync(v => v.CustomFieldDefinitionId == id) });
            }

            // Check FormatMask changed
            var oldMask = oldOpts?.FormatMask;
            var newMask = request.Validation?.FormatMask;
            if (oldMask != newMask)
            {
                return Results.Conflict(new { code = "RULE_CHANGE_BLOCKED", message = "Cannot change format mask because data exists.", affectedCount = await db.CustomFieldValues.CountAsync(v => v.CustomFieldDefinitionId == id) });
            }

            // Check MaxLength decreased
            if (request.Validation?.MaxLength.HasValue == true)
            {
                var newMax = request.Validation.MaxLength.Value;
                var affectedCount = await db.CustomFieldValues.CountAsync(v => v.CustomFieldDefinitionId == id && v.Value.Length > newMax);
                if (affectedCount > 0)
                {
                    return Results.Conflict(new { code = "RULE_CHANGE_BLOCKED", message = "Cannot decrease MaxLength because existing values are longer.", affectedCount });
                }
            }

            // Check MinLength increased
            if (request.Validation?.MinLength.HasValue == true)
            {
                var newMin = request.Validation.MinLength.Value;
                var affectedCount = await db.CustomFieldValues.CountAsync(v => v.CustomFieldDefinitionId == id && v.Value.Length < newMin);
                if (affectedCount > 0)
                {
                    return Results.Conflict(new { code = "RULE_CHANGE_BLOCKED", message = "Cannot increase MinLength because existing values are shorter.", affectedCount });
                }
            }

            // Check Choices option removed
            if (def.FieldType == CustomFieldType.SingleChoice && request.Validation?.Choices is not null)
            {
                var oldChoices = oldOpts?.Choices ?? new List<string>();
                var newChoices = request.Validation.Choices;
                var removedChoices = oldChoices.Except(newChoices).ToList();
                if (removedChoices.Count > 0)
                {
                    var affectedCount = await db.CustomFieldValues.CountAsync(v => v.CustomFieldDefinitionId == id && removedChoices.Contains(v.Value));
                    if (affectedCount > 0)
                    {
                        return Results.Conflict(new { code = "RULE_CHANGE_BLOCKED", message = "Cannot remove choices that are currently in use.", affectedCount });
                    }
                }
            }
        }

        def.Label = request.Label;
        def.HelpText = request.HelpText;
        def.IsRequired = request.IsRequired;
        def.IsCandidateFacing = request.IsCandidateFacing;
        def.ValidationJson = SerializeValidation(request.Validation);
        def.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var valueCount = await db.CustomFieldValues.CountAsync(v => v.CustomFieldDefinitionId == def.Id);
        return Results.Ok(MapToDto(def, valueCount));
    }

    private static async Task<IResult> DisableCustomField(Guid id, MicroDbContext db)
    {
        var def = await db.CustomFieldDefinitions.FindAsync(id);
        if (def is null) return Results.NotFound();

        def.IsDisabled = true;
        def.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> EnableCustomField(Guid id, MicroDbContext db)
    {
        var def = await db.CustomFieldDefinitions.FindAsync(id);
        if (def is null) return Results.NotFound();

        def.IsDisabled = false;
        def.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> ReorderCustomFields(ReorderCustomFieldsRequest request, MicroDbContext db)
    {
        if (request.OrderedIds is null || request.OrderedIds.Count == 0)
            return Results.BadRequest("OrderedIds list cannot be null or empty.");

        var definitions = await db.CustomFieldDefinitions
            .Where(d => request.OrderedIds.Contains(d.Id))
            .ToListAsync();

        if (definitions.Count == 0)
            return Results.NotFound("No definitions found matching the provided IDs.");

        // Verify all definitions belong to the same TargetEntity scope group
        var targetEntities = definitions.Select(d => d.TargetEntity).Distinct().ToList();
        if (targetEntities.Count > 1)
            return Results.UnprocessableEntity("All IDs in the reorder request must belong to the same target entity scope group.");

        for (var i = 0; i < request.OrderedIds.Count; i++)
        {
            var defId = request.OrderedIds[i];
            var def = definitions.FirstOrDefault(d => d.Id == defId);
            if (def is not null)
            {
                def.Order = i + 1;
                def.UpdatedAt = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCustomField(Guid id, MicroDbContext db)
    {
        var def = await db.CustomFieldDefinitions.FindAsync(id);
        if (def is null) return Results.NotFound();

        var hasValues = await db.CustomFieldValues.AnyAsync(v => v.CustomFieldDefinitionId == id);
        if (hasValues)
        {
            return Results.Conflict(new { code = "FIELD_HAS_VALUES", message = "Cannot delete a custom field that already has recorded values." });
        }

        db.CustomFieldDefinitions.Remove(def);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    // Helper validation
    private static Dictionary<string, string[]> ValidateRequest(
        string label,
        string? helpText,
        CustomFieldTargetEntity targetEntity,
        CustomFieldType fieldType,
        bool isCandidateFacing,
        ValidationOptionsDto? validation)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(label) || label.Length > 200)
            errors["label"] = ["Label must be between 1 and 200 characters."];

        if (helpText?.Length > 500)
            errors["helpText"] = ["Help text must be at most 500 characters."];

        if (isCandidateFacing && targetEntity is not (CustomFieldTargetEntity.Application_Global or CustomFieldTargetEntity.Application_Applied))
            errors["isCandidateFacing"] = ["Only General Application or Application in the Applied stage fields can be marked as candidate facing."];

        if (fieldType == CustomFieldType.SingleChoice && (validation?.Choices is null || validation.Choices.Count == 0))
            errors["validation.choices"] = ["Single choice options must contain at least one option."];

        if (!string.IsNullOrEmpty(validation?.FormatMask))
        {
            if (fieldType != CustomFieldType.ShortText)
                errors["validation.formatMask"] = ["FormatMask can only be configured for ShortText."];
            if (validation.Presets is { Count: > 0 })
                errors["validation.formatMask"] = ["FormatMask and Presets are mutually exclusive."];
        }

        if (validation?.Presets is { Count: > 0 })
        {
            var invalidPresets = validation.Presets.Where(p => !ValidatorRegistry.Presets.ContainsKey(p)).ToList();
            if (invalidPresets.Count > 0)
                errors["validation.presets"] = [$"Invalid presets: {string.Join(", ", invalidPresets)}."];
        }

        return errors;
    }

    private static CustomFieldDefinitionDto MapToDto(CustomFieldDefinition def, int valueCount)
    {
        var validationDto = string.IsNullOrEmpty(def.ValidationJson)
            ? null
            : JsonSerializer.Deserialize<ValidationOptionsDto>(def.ValidationJson);

        return new CustomFieldDefinitionDto(
            def.Id,
            def.TargetEntity,
            def.FieldType,
            def.Label,
            def.HelpText,
            def.Order,
            def.IsRequired,
            def.IsDisabled,
            def.IsCandidateFacing,
            validationDto,
            valueCount,
            def.CreatedAt,
            def.UpdatedAt
        );
    }

    private static string? SerializeValidation(ValidationOptionsDto? validation)
    {
        if (validation is null) return null;
        return JsonSerializer.Serialize(new ValidationOptions
        {
            MinLength = validation.MinLength,
            MaxLength = validation.MaxLength,
            Min = validation.Min,
            Max = validation.Max,
            MinDate = validation.MinDate,
            MaxDate = validation.MaxDate,
            Presets = validation.Presets,
            FormatMask = validation.FormatMask,
            Choices = validation.Choices
        });
    }
}
