using Scalar.AspNetCore;

namespace Micro.API.Infrastructure.OpenApi;

public static class SwaggerExtensions
{
    public static void AddSwagger(this IServiceCollection services)
    {
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();
    }

    public static void UseSwagger(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/swagger");
        }
    }
}
