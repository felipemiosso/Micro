using System.Net.Http.Json;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class HealthCheckTests : IntegrationTestBase
{
    public HealthCheckTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Get_Health_ReturnsHealthy()
    {
        // Arrange
        var loginRequest = new { Email = "admin@microats.com", Password = "AdminPassword123!" };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthTests.LoginResponse>();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(content);
        Assert.Equal("Healthy", content.status);
    }

    private record HealthResponse(string status);
}
