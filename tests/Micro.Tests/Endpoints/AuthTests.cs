using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class AuthTests : IntegrationTestBase
{
    public AuthTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginRequest = new { Email = "admin@microats.com", Password = "AdminPassword123!" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(result?.Token);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new { Email = "admin@microats.com", Password = "WrongPassword" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public record LoginResponse(string Token);
}
