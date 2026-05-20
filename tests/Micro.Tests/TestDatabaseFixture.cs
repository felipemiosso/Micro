using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Npgsql;
using Micro.API.Data;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;

namespace Micro.Tests;

public class TestDatabaseFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> Factory { get; }
    public string ConnectionString { get; private set; } = string.Empty;
    public Respawner Respawner { get; private set; } = null!;
    public AuthTestHelper Auth { get; private set; } = null!;

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
                    ["Serilog:MinimumLevel:Override:Microsoft.EntityFrameworkCore.Database.Connection"] = "Fatal",
                    ["Firebase:ProjectId"] = "demo-micro-ats",
                    ["Firebase:UseEmulator"] = "true",
                    ["Firebase:EmulatorHost"] = "localhost:9099"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                // No longer overriding auth with TestAuthHandler
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        ConnectionString = config.GetConnectionString("DefaultConnection")!;

        var projectId = config["Firebase:ProjectId"] ?? "test-project";
        var emulatorHost = config["Firebase:EmulatorHost"] ?? "localhost:9099";
        
        // Initialize AuthTestHelper
        Auth = new AuthTestHelper(projectId, emulatorHost);

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
        
        // Re-seed essential data after reset
        using var scope = Factory.Services.CreateScope();
        await DbInitializer.SeedLookups(scope.ServiceProvider);
        await DbInitializer.SeedUser(scope.ServiceProvider);
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
