using System.Linq;
using System.Text.RegularExpressions;

namespace Micro.API.Infrastructure.CustomFields.Presets;

public static class PhoneMobileValidator
{
    public static ValidatorRegistry.ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new(false, "Mobile phone cannot be empty");

        // Format check: (XX) 9XXXX-XXXX or raw 11 digits starting with 9 after area code
        var regex = new Regex(@"^\(\d{2}\)\s?9\d{4}-\d{4}$|^\d{11}$");
        if (!regex.IsMatch(value))
            return new(false, "Mobile phone must be in (XX) 9XXXX-XXXX format or contain exactly 11 digits");

        var clean = new string(value.Where(char.IsDigit).ToArray());
        if (clean.Length != 11 || clean[2] != '9')
            return new(false, "Invalid mobile phone");

        return new(true);
    }
}
