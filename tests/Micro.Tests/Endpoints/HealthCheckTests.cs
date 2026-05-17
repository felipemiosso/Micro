using System.Net.Http.Json;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class HealthCheckTests : IntegrationTestBase
{
    public HealthCheckTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    private void Authenticate()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");
    }

    [Fact]
    public async Task Get_Health_ReturnsHealthy()
    {
        // Arrange
        Authenticate();

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
