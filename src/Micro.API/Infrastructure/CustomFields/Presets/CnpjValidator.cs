using System.Linq;
using System.Text.RegularExpressions;

namespace Micro.API.Infrastructure.CustomFields.Presets;

public static class CnpjValidator
{
    public static ValidatorRegistry.ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new(false, "CNPJ cannot be empty");

        // Format check: "00.000.000/0000-00" or raw 14 digits
        var regex = new Regex(@"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$|^\d{14}$");
        if (!regex.IsMatch(value))
            return new(false, "CNPJ must be in 00.000.000/0000-00 format or contain exactly 14 digits");

        var cleanCnpj = new string(value.Where(char.IsDigit).ToArray());
        if (cleanCnpj.Length != 14)
            return new(false, "CNPJ must contain exactly 14 digits");

        if (cleanCnpj.Distinct().Count() == 1)
            return new(false, "CNPJ with repeated digits is invalid");

        var tempCnpj = cleanCnpj[..12];
        var sum = 0;
        var multipliers1 = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        for (var i = 0; i < 12; i++)
            sum += (tempCnpj[i] - '0') * multipliers1[i];

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        tempCnpj += digit1;
        sum = 0;
        var multipliers2 = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        for (var i = 0; i < 13; i++)
            sum += (tempCnpj[i] - '0') * multipliers2[i];

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        if (cleanCnpj.EndsWith($"{digit1}{digit2}"))
            return new(true);

        return new(false, "Invalid CNPJ");
    }
}
