using System.Reflection;
using System.Security.Claims;

namespace Micro.API.Infrastructure.Auth;

public record AuthUser(string Id, string Email, string? Name, string? PhotoUrl)
{
    public static ValueTask<AuthUser?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return ValueTask.FromResult<AuthUser?>(null);
        }

        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(email))
        {
            return ValueTask.FromResult<AuthUser?>(null);
        }

        var name = user.FindFirst(ClaimTypes.Name)?.Value;
        var photoUrl = user.FindFirst("picture")?.Value;

        return ValueTask.FromResult<AuthUser?>(new AuthUser(id, email, name, photoUrl));
    }
}
