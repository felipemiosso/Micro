using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Micro.API.Infrastructure.Auth;

public static class AuthExtensions
{
    public static void AddAuth(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var projectId = configuration["Firebase:ProjectId"];
        var useEmulator = configuration.GetValue<bool>("Firebase:UseEmulator") && !environment.IsProduction();

        var authBuilder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

        if (useEmulator)
        {
            authBuilder.AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://securetoken.google.com/{projectId}",
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = false,
                    RequireSignedTokens = false,
                    RoleClaimType = "role"
                };
            });
        }
        else
        {
            authBuilder.AddJwtBearer(options =>
            {
                options.Authority = $"https://securetoken.google.com/{projectId}";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://securetoken.google.com/{projectId}",
                    ValidateAudience = true,
                    ValidAudience = projectId,
                    ValidateLifetime = true,
                    RequireSignedTokens = true
                };
            });
        }

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });

        // Dynamic Permission-based Authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
    }
}
