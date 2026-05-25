using System.ComponentModel.DataAnnotations;

namespace Micro.API.Infrastructure.CustomFields.Presets;

public static class EmailValidator
{
    private static readonly EmailAddressAttribute EmailAttribute = new();

    public static ValidatorRegistry.ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new(false, "Email cannot be empty");

        if (EmailAttribute.IsValid(value))
            return new(true);

        return new(false, "Invalid email address");
    }
}
