using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Npgsql;
using Micro.API.Data;

namespace Micro.Tests;

public class TestDatabaseFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> Factory { get; }
    public string ConnectionString { get; private set; } = string.Empty;
    public Respawner Respawner { get; private set; } = null!;

    public TestDatabaseFixture()
    {
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=MicroATS_Tests;Username=postgres;Password=postgres",
                    ["Serilog:MinimumLevel:Override:Microsoft.EntityFrameworkCore.Database.Connection"] = "Fatal"
                });
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        ConnectionString = config.GetConnectionString("DefaultConnection")!;

        var dbContext = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
        
        // Clear pools to avoid "database is being accessed by other users" errors
        NpgsqlConnection.ClearAllPools();

        // Ensure database is clean and has schema
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();

        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        Respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await Respawner.ResetAsync(conn);
        
        // Re-seed admin user after reset if needed, 
        // but since we want clean state for tests, we let tests handle seeding or use DbInitializer
        using var scope = Factory.Services.CreateScope();
        await DbInitializer.SeedAdminUser(scope.ServiceProvider);
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }
}

[CollectionDefinition("TestDatabase")]
public class TestDatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
}
