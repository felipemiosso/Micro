using System.Security.Claims;

namespace Micro.API.Infrastructure.Auth;

public record AuthUser(string Id, string Email, string? Name, string? PhotoUrl)
{
    public static ValueTask<AuthUser?> BindAsync(HttpContext context)
    {
        var user = context.User;
        
        var id = user.FindFirst("user_id")?.Value;
        var email = user.FindFirst("email")?.Value;

        if (id is null || email is null)
        {
            return ValueTask.FromResult<AuthUser?>(null);
        }

        var name = user.FindFirst("name")?.Value;
        var photoUrl = user.FindFirst("picture")?.Value;

        return ValueTask.FromResult<AuthUser?>(new AuthUser(id, email, name, photoUrl));
    }
}
