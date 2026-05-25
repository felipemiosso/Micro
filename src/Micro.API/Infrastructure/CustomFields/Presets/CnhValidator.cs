using System.Linq;
using System.Text.RegularExpressions;

namespace Micro.API.Infrastructure.CustomFields.Presets;

public static class CnhValidator
{
    public static ValidatorRegistry.ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new(false, "CNH cannot be empty");

        var clean = new string(value.Where(char.IsDigit).ToArray());
        if (clean.Length != 11 || clean.Distinct().Count() == 1)
            return new(false, "CNH must contain exactly 11 digits");

        return new(true);
    }
}
