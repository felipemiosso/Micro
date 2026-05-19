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
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if (env.IsEnvironment("Testing"))
        {
            // For E2E tests, we want a fresh start
            await dbContext.Database.EnsureDeletedAsync();
        }

        await dbContext.Database.MigrateAsync();
        await DbInitializer.SeedData(scope.ServiceProvider);
    }
}
