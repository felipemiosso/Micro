using System.Linq;
using System.Text.RegularExpressions;

namespace Micro.API.Infrastructure.CustomFields.Presets;

public static class CepValidator
{
    public static ValidatorRegistry.ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new(false, "CEP cannot be empty");

        // Format check: 00000-000 or raw 8 digits
        var regex = new Regex(@"^\d{5}-\d{3}$|^\d{8}$");
        if (!regex.IsMatch(value))
            return new(false, "CEP must be in 00000-000 format or contain exactly 8 digits");

        var clean = new string(value.Where(char.IsDigit).ToArray());
        if (clean.Length != 8)
            return new(false, "Invalid CEP");

        return new(true);
    }
}
