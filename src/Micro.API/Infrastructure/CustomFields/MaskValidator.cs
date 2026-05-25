using System.Linq;

namespace Micro.API.Infrastructure.CustomFields;

public static class MaskValidator
{
    // # = digit, A = letter, X = alphanumeric, else literal
    public static bool Matches(string mask, string value)
    {
        if (string.IsNullOrEmpty(mask) || string.IsNullOrEmpty(value)) return false;
        if (mask.Length != value.Length) return false;
        return mask.Zip(value).All(pair =>
            pair.First switch
            {
                '#' => char.IsDigit(pair.Second),
                'A' => char.IsLetter(pair.Second),
                'X' => char.IsLetterOrDigit(pair.Second),
                _   => pair.First == pair.Second
            });
    }
}
