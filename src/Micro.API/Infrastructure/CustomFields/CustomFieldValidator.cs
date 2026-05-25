using System;
using System.Collections.Generic;
using System.Linq;
using Micro.API.Data.Models;

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
            errors.Add($"'{def.Label}' is required.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(value)) return errors;

        switch (def.FieldType)
        {
            case CustomFieldType.ShortText:
            case CustomFieldType.LongText:
                if (opts?.MinLength.HasValue == true && value.Length < opts.MinLength)
                    errors.Add($"'{def.Label}' must be at least {opts.MinLength} characters.");
                if (opts?.MaxLength.HasValue == true && value.Length > opts.MaxLength)
                    errors.Add($"'{def.Label}' must be at most {opts.MaxLength} characters.");

                if (def.FieldType == CustomFieldType.ShortText)
                {
                    if (opts?.Presets is { Count: > 0 })
                    {
                        var result = ValidatorRegistry.ValidateAny(opts.Presets, value);
                        if (!result.IsValid)
                            errors.Add($"'{def.Label}': {result.ErrorMessage ?? "invalid format"}.");
                    }
                    else if (!string.IsNullOrEmpty(opts?.FormatMask))
                    {
                        if (!MaskValidator.Matches(opts.FormatMask, value))
                            errors.Add($"'{def.Label}' does not match the expected format ({opts.FormatMask}).");
                    }
                }
                break;

            case CustomFieldType.Number:
                if (!decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var num))
                {
                    errors.Add($"'{def.Label}' must be a valid number.");
                    break;
                }
                if (opts?.Min.HasValue == true && num < opts.Min)
                    errors.Add($"'{def.Label}' must be greater than or equal to {opts.Min}.");
                if (opts?.Max.HasValue == true && num > opts.Max)
                    errors.Add($"'{def.Label}' must be less than or equal to {opts.Max}.");
                break;

            case CustomFieldType.Date:
                if (!DateOnly.TryParse(value, out var date))
                {
                    errors.Add($"'{def.Label}' must be a valid date.");
                    break;
                }
                if (opts?.MinDate.HasValue == true && date < opts.MinDate)
                    errors.Add($"'{def.Label}' must be on or after {opts.MinDate:yyyy-MM-dd}.");
                if (opts?.MaxDate.HasValue == true && date > opts.MaxDate)
                    errors.Add($"'{def.Label}' must be on or before {opts.MaxDate:yyyy-MM-dd}.");
                break;

            case CustomFieldType.Boolean:
                if (value is not ("true" or "false"))
                    errors.Add($"'{def.Label}' must be true or false.");
                break;

            case CustomFieldType.SingleChoice:
                if (opts?.Choices is { Count: > 0 } && !opts.Choices.Contains(value))
                    errors.Add($"'{def.Label}' contains an invalid value.");
                break;
        }

        return errors;
    }

    private static ValidationOptions? DeserializeOptions(string? json) =>
        string.IsNullOrEmpty(json)
            ? null
            : System.Text.Json.JsonSerializer.Deserialize<ValidationOptions>(json);
}
