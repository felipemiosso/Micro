using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Endpoints.UserProfile;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class UserProfileEndpointsTests : IntegrationTestBase
{
    public UserProfileEndpointsTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetProfile_ReturnsProfile_WhenUserExists()
    {
        // Arrange
        var email = $"profile-{Guid.NewGuid()}@example.com";
        await AuthenticateAsync(email);
        
        // Lookup generated UID from Firebase Auth
        var userRecord = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
        var uid = userRecord.Uid;
        
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var user = new User { Id = uid, Email = email, FullName = "Profile User" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await Client.GetAsync("/api/profile");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>(JsonOptions);
        Assert.NotNull(profile);
        Assert.Equal(uid, profile.Id);
        Assert.Equal(email, profile.Email);
        Assert.Equal("Profile User", profile.FullName);
    }

    [Fact]
    public async Task SyncProfile_CreatesNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var email = $"sync-{Guid.NewGuid()}@example.com";
        await AuthenticateAsync(email);
        
        var userRecord = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
        var uid = userRecord.Uid;

        // Act
        var response = await Client.PostAsync("/api/profile", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>(JsonOptions);
        Assert.NotNull(profile);
        Assert.Equal(uid, profile.Id);
        Assert.Equal(email, profile.Email);

        using var scope = Fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
        var userInDb = await db.Users.FindAsync(uid);
        Assert.NotNull(userInDb);
        Assert.Equal(email, userInDb.Email);
    }
}
