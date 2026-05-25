using System.Linq;
using System.Text.RegularExpressions;

namespace Micro.API.Infrastructure.CustomFields.Presets;

public static class CpfValidator
{
    public static ValidatorRegistry.ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new(false, "CPF cannot be empty");

        // Format validation: either exact mask "000.000.000-00" or raw 11 digits
        var regex = new Regex(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$|^\d{11}$");
        if (!regex.IsMatch(value))
            return new(false, "CPF must be in 000.000.000-00 format or contain exactly 11 digits");

        var cleanCpf = new string(value.Where(char.IsDigit).ToArray());
        if (cleanCpf.Length != 11)
            return new(false, "CPF must contain exactly 11 digits");

        // CPF cannot have all identical digits (e.g. 111.111.111-11)
        if (cleanCpf.Distinct().Count() == 1)
            return new(false, "CPF with repeated digits is invalid");

        var tempCpf = cleanCpf[..9];
        var sum = 0;
        var multipliers1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        for (var i = 0; i < 9; i++)
            sum += (tempCpf[i] - '0') * multipliers1[i];

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        tempCpf += digit1;
        sum = 0;
        var multipliers2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        for (var i = 0; i < 10; i++)
            sum += (tempCpf[i] - '0') * multipliers2[i];

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        if (cleanCpf.EndsWith($"{digit1}{digit2}"))
            return new(true);

        return new(false, "Invalid CPF");
    }
}
