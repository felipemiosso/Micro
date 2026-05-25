using System.Linq;
using System.Text.RegularExpressions;

namespace Micro.API.Infrastructure.CustomFields.Presets;

public static class PhoneLandlineValidator
{
    public static ValidatorRegistry.ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new(false, "Landline phone cannot be empty");

        // Format check: (XX) XXXX-XXXX or raw 10 digits
        var regex = new Regex(@"^\(\d{2}\)\s?[2-8]\d{3}-\d{4}$|^\d{10}$");
        if (!regex.IsMatch(value))
            return new(false, "Landline phone must be in (XX) XXXX-XXXX format or contain exactly 10 digits");

        var clean = new string(value.Where(char.IsDigit).ToArray());
        if (clean.Length != 10)
            return new(false, "Invalid landline phone");

        return new(true);
    }
}
