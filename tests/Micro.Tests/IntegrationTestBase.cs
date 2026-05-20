using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Micro.Tests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly TestDatabaseFixture Fixture;
    protected readonly HttpClient Client;
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected IntegrationTestBase(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
        Client = fixture.Factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
    }

    protected async Task AuthenticateAsync(string? email = null, string password = "password123", Dictionary<string, object>? roles = null)
    {
        email ??= $"test-{Guid.NewGuid()}@microats.com";
        roles ??= new Dictionary<string, object> { { "role", "Admin" } };
        var token = await Fixture.Auth.CreateUserAndGetTokenAsync(email, password, roles);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;
}
