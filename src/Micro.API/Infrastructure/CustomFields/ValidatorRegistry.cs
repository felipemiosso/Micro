using System;
using System.Collections.Generic;
using System.Linq;
using Micro.API.Infrastructure.CustomFields.Presets;

namespace Micro.API.Infrastructure.CustomFields;

public static class ValidatorRegistry
{
    public record PresetDefinition(string Label, string Tag, Func<string, ValidationResult> Validate);
    public record ValidationResult(bool IsValid, string? ErrorMessage = null);

    public static readonly IReadOnlyDictionary<string, PresetDefinition> Presets =
        new Dictionary<string, PresetDefinition>
        {
            ["cpf"]            = new("CPF",               "Documents",   CpfValidator.Validate),
            ["cnpj"]           = new("CNPJ",              "Documents",   CnpjValidator.Validate),
            ["pis"]            = new("PIS / PASEP",        "Documents",   PisValidator.Validate),
            ["cnh"]            = new("CNH",               "Documents",   CnhValidator.Validate),
            ["email"]          = new("Email",             "Contact",     EmailValidator.Validate),
            ["phone_mobile"]   = new("Mobile Phone",      "Contact",     PhoneMobileValidator.Validate),
            ["phone_landline"] = new("Landline Phone",    "Contact",     PhoneLandlineValidator.Validate),
            ["url"]            = new("URL",               "Contact",     UrlValidator.Validate),
            ["cep"]            = new("CEP",               "Location",    CepValidator.Validate),
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
