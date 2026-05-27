using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Endpoints.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class UserEndpointsTests : IntegrationTestBase
{
    public UserEndpointsTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetUsers_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>(JsonOptions);
        Assert.NotNull(users);
        Assert.NotEmpty(users);
    }

    [Fact]
    public async Task InviteUser_CreatesRecord_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var email = $"invite-{Guid.NewGuid()}@example.com";
        var request = new InviteUserRequest(email, "New Invited User", new List<Guid>());

        // Act
        var response = await Client.PostAsJsonAsync("/api/users/invite", request, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        using var scope = Fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
        var userInDb = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        Assert.NotNull(userInDb);
        Assert.Equal("New Invited User", userInDb.FullName);
        Assert.True(userInDb.IsInvitePending);
    }

    [Fact]
    public async Task ManageRoles_UpdatesRoles_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();

        string testUserId;
        Guid roleId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var role = new Role { Id = Guid.NewGuid(), Name = $"Role-{Guid.NewGuid()}" };
            db.Roles.Add(role);

            var user = new User { Id = $"uid-{Guid.NewGuid()}", Email = $"user-{Guid.NewGuid()}@example.com", FullName = "Role User" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            testUserId = user.Id;
            roleId = role.Id;
        }

        var request = new ManageUserRolesRequest(new List<Guid> { roleId });

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{testUserId}/roles", request, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var userWithRoles = await db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == testUserId);
            Assert.NotNull(userWithRoles);
            Assert.Contains(userWithRoles.Roles, r => r.Id == roleId);
        }
    }

    [Fact]
    public async Task DeleteUser_RemovesRecord_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();

        string testUserId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var user = new User { Id = $"uid-{Guid.NewGuid()}", Email = $"user-{Guid.NewGuid()}@example.com", FullName = "Delete User" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            testUserId = user.Id;
        }

        // Act
        var response = await Client.DeleteAsync($"/api/users/{testUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var userInDb = await db.Users.FindAsync(testUserId);
            Assert.Null(userInDb);
        }
    }
}
