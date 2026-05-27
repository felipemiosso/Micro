using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class AdminEndpointsTests : IntegrationTestBase
{
    public AdminEndpointsTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    // Departments
    [Fact]
    public async Task GetDepartments_ReturnsList()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/admin/departments");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var depts = await response.Content.ReadFromJsonAsync<List<Department>>(JsonOptions);
        Assert.NotNull(depts);
        Assert.NotEmpty(depts);
    }

    [Fact]
    public async Task CreateDepartment_ValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var dept = new Department { Name = $"Dept-{Guid.NewGuid()}" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/admin/departments", dept, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Department>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(dept.Name, created.Name);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task UpdateDepartment_ValidData_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        Guid deptId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var dept = new Department { Id = Guid.NewGuid(), Name = "Old Name" };
            db.Departments.Add(dept);
            await db.SaveChangesAsync();
            deptId = dept.Id;
        }

        var update = new Department { Name = "New Name", IsActive = false };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/admin/departments/{deptId}", update, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var updated = await db.Departments.FindAsync(deptId);
            Assert.NotNull(updated);
            Assert.Equal("New Name", updated.Name);
            Assert.False(updated.IsActive);
        }
    }

    // Salary Bands
    [Fact]
    public async Task GetSalaryBands_ReturnsList()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/admin/salary-bands");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bands = await response.Content.ReadFromJsonAsync<List<SalaryBand>>(JsonOptions);
        Assert.NotNull(bands);
        Assert.NotEmpty(bands);
    }

    [Fact]
    public async Task CreateSalaryBand_ValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var band = new SalaryBand { Name = $"Band-{Guid.NewGuid()}", MinAmount = 10000, MaxAmount = 20000, Currency = "USD" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/admin/salary-bands", band, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SalaryBand>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(band.Name, created.Name);
        Assert.Equal(10000, created.MinAmount);
    }

    [Fact]
    public async Task UpdateSalaryBand_ValidData_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        Guid bandId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var band = new SalaryBand { Id = Guid.NewGuid(), Name = "Old Band", MinAmount = 1000, MaxAmount = 2000, Currency = "USD" };
            db.SalaryBands.Add(band);
            await db.SaveChangesAsync();
            bandId = band.Id;
        }

        var update = new SalaryBand { Name = "New Band", MinAmount = 3000, MaxAmount = 4000, Currency = "EUR" };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/admin/salary-bands/{bandId}", update, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var updated = await db.SalaryBands.FindAsync(bandId);
            Assert.NotNull(updated);
            Assert.Equal("New Band", updated.Name);
            Assert.Equal(3000, updated.MinAmount);
            Assert.Equal("EUR", updated.Currency);
        }
    }

    // Cost Centers
    [Fact]
    public async Task GetCostCenters_ReturnsList()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/admin/cost-centers");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var centers = await response.Content.ReadFromJsonAsync<List<CostCenter>>(JsonOptions);
        Assert.NotNull(centers);
        Assert.NotEmpty(centers);
    }

    [Fact]
    public async Task CreateCostCenter_ValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var center = new CostCenter { Code = "CC-TEST", Name = "Test Center" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/admin/cost-centers", center, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CostCenter>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(center.Code, created.Code);
        Assert.Equal(center.Name, created.Name);
    }

    [Fact]
    public async Task UpdateCostCenter_ValidData_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        Guid centerId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var center = new CostCenter { Id = Guid.NewGuid(), Code = "OLD-CC", Name = "Old CC" };
            db.CostCenters.Add(center);
            await db.SaveChangesAsync();
            centerId = center.Id;
        }

        var update = new CostCenter { Code = "NEW-CC", Name = "New CC" };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/admin/cost-centers/{centerId}", update, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var updated = await db.CostCenters.FindAsync(centerId);
            Assert.NotNull(updated);
            Assert.Equal("NEW-CC", updated.Code);
            Assert.Equal("New CC", updated.Name);
        }
    }
}
