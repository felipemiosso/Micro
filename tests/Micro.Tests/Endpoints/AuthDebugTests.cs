using System.Net;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class AuthDebugTests : IntegrationTestBase
{
    public AuthDebugTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task SimpleAuth_ShouldWork()
    {
        // 1. Authenticate using the emulator
        var email = $"test-{Guid.NewGuid()}@microats.com";
        var roles = new Dictionary<string, object> { { "role", "Admin" } };
        var token = await Fixture.Auth.CreateUserAndGetTokenAsync(email, "password123", roles);
        
        // LOG TOKEN FOR DEBUGGING
        Console.WriteLine($"[DEBUG] Generated Token: {token}");
        
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Try to hit a simple protected endpoint
        var response = await Client.GetAsync("/api/requisitions");

        // 3. Assert
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] 401 Unauthorized response body: {body}");
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
