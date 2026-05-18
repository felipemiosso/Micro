using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Micro.API.Infrastructure.Auth;

public static class FirebaseEmulatorMiddlewareExtensions
{
    public static IApplicationBuilder UseFirebaseEmulatorAuth(this IApplicationBuilder app, IConfiguration config, IWebHostEnvironment env)
    {
        if (env.IsProduction() || !config.GetValue<bool>("Firebase:UseEmulator"))
        {
            return app;
        }

        return app.Use(async (context, next) =>
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authStr = authHeader.ToString();
                if (authStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authStr.Substring("Bearer ".Length).Trim();
                    if (!string.IsNullOrEmpty(token) && token.Split('.').Length == 3)
                    {
                        try
                        {
                            var handler = new JsonWebTokenHandler();
                            var jwt = handler.ReadJsonWebToken(token);
                            var claims = jwt.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();
                            var identity = new ClaimsIdentity(claims, "FirebaseEmulator");
                            context.User = new ClaimsPrincipal(identity);
                        }
                        catch
                        {
                            // Ignore invalid tokens
                        }
                    }
                }
            }
            await next();
        });
    }
}
