using System;

namespace Micro.API.Infrastructure.CustomFields.Presets;

public static class UrlValidator
{
    public static ValidatorRegistry.ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new(false, "URL cannot be empty");

        if (Uri.TryCreate(value, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return new(true);
        }

        return new(false, "Invalid URL");
    }
}
