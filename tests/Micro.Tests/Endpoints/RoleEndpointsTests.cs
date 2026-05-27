using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Endpoints.Role;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class RoleEndpointsTests : IntegrationTestBase
{
    public RoleEndpointsTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetRoles_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsync();
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            db.Roles.Add(new Role { Id = Guid.NewGuid(), Name = "Test Role" });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await Client.GetAsync("/api/roles");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var roles = await response.Content.ReadFromJsonAsync<List<RoleResponse>>(JsonOptions);
        Assert.NotNull(roles);
        Assert.NotEmpty(roles);
    }

    [Fact]
    public async Task CreateRole_ValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateRoleRequest($"TestRole-{Guid.NewGuid()}", new List<string> { "Candidate:View", "Application:View" });

        // Act
        var response = await Client.PostAsJsonAsync("/api/roles", request, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<RoleResponse>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(request.Permissions, created.Permissions);
    }

    [Fact]
    public async Task GetAvailableActions_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/roles/available-actions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var actions = await response.Content.ReadFromJsonAsync<List<object>>(JsonOptions);
        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
    }
}
