using System.Linq;
using System.Text.RegularExpressions;

namespace Micro.API.Infrastructure.CustomFields.Presets;

public static class PisValidator
{
    public static ValidatorRegistry.ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new(false, "PIS/PASEP cannot be empty");

        // Format check: "000.00000.00-0" or raw 11 digits
        var regex = new Regex(@"^\d{3}\.\d{5}\.\d{2}-\d{1}$|^\d{11}$");
        if (!regex.IsMatch(value))
            return new(false, "PIS/PASEP must be in 000.00000.00-0 format or contain exactly 11 digits");

        var cleanPis = new string(value.Where(char.IsDigit).ToArray());
        if (cleanPis.Length != 11)
            return new(false, "PIS/PASEP must contain exactly 11 digits");

        if (cleanPis.Distinct().Count() == 1)
            return new(false, "PIS/PASEP with repeated digits is invalid");

        var multipliers = new[] { 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (cleanPis[i] - '0') * multipliers[i];

        var remainder = sum % 11;
        var digit = 11 - remainder;
        if (digit == 10 || digit == 11)
            digit = 0;

        if (cleanPis.EndsWith(digit.ToString()))
            return new(true);

        return new(false, "Invalid PIS/PASEP");
    }
}
