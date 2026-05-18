using Micro.API.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Micro.API.Infrastructure.Database;

public static class DatabaseExtensions
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MicroDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            Log.Information("Using connection string: {ConnectionString}", connectionString);
            options.UseNpgsql(connectionString);
        });
    }

    public static async Task ApplyMigrationsAndSeed(this IApplicationBuilder app)
    {
        if (app is WebApplication webApp && webApp.Environment.IsEnvironment("Testing"))
        {
            return;
        }

        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
        await dbContext.Database.MigrateAsync();
        await DbInitializer.SeedData(scope.ServiceProvider);
    }
}
