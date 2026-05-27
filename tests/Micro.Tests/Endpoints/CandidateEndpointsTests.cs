using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Endpoints.Candidate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class CandidateEndpointsTests : IntegrationTestBase
{
    public CandidateEndpointsTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetCandidateDetail_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsync();

        Guid candidateId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var candidate = new Candidate
            {
                Id = Guid.NewGuid(),
                FullName = "Alice Smith",
                Email = $"alice-{Guid.NewGuid()}@example.com",
                Phone = "1234567890"
            };
            db.Candidates.Add(candidate);
            await db.SaveChangesAsync();
            candidateId = candidate.Id;
        }

        // Act
        var response = await Client.GetAsync($"/api/candidates/{candidateId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<CandidateDetailResponse>(JsonOptions);
        Assert.NotNull(detail);
        Assert.Equal("Alice Smith", detail.FullName);
    }

    [Fact]
    public async Task GetCandidateDetail_NotFound_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/candidates/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCandidateDetail_WithApplicationsAndFeedbacks_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsync();

        Guid candidateId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var dept = await db.Departments.FirstAsync();
            var band = await db.SalaryBands.FirstAsync();
            var cc = await db.CostCenters.FirstAsync();

            var requisition = new Requisition
            {
                Id = Guid.NewGuid(),
                Title = "Dev Requisition",
                DepartmentId = dept.Id,
                SalaryBandId = band.Id,
                CostCenterId = cc.Id,
                OpeningsCount = 1,
                EmploymentType = EmploymentType.FullTime,
                WorkplaceType = WorkplaceType.OnSite,
                Location = "Remote",
                JobDescription = "Description",
                Status = RequisitionStatus.Finalized,
                CreatedBy = "Admin",
                HiringManagerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
            db.Requisitions.Add(requisition);

            var jobPosting = new JobPosting
            {
                Id = Guid.NewGuid(),
                RequisitionId = requisition.Id,
                Title = "Dev Job",
                Description = "Description",
                Status = JobPostingStatus.Published,
                CreatedAt = DateTime.UtcNow
            };
            db.JobPostings.Add(jobPosting);

            var candidate = new Candidate
            {
                Id = Guid.NewGuid(),
                FullName = "Alice Cooper",
                Email = $"cooper-{Guid.NewGuid()}@example.com",
                Phone = "9876543210"
            };
            db.Candidates.Add(candidate);

            var application = new Application
            {
                Id = Guid.NewGuid(),
                CandidateId = candidate.Id,
                JobPostingId = jobPosting.Id,
                Status = ApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow
            };
            db.Applications.Add(application);

            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                Notes = "Excellent skills",
                Score = 5,
                AdminId = "Admin",
                CreatedAt = DateTime.UtcNow
            };
            db.Feedbacks.Add(feedback);

            await db.SaveChangesAsync();
            candidateId = candidate.Id;
        }

        // Act
        var response = await Client.GetAsync($"/api/candidates/{candidateId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<CandidateDetailResponse>(JsonOptions);
        Assert.NotNull(detail);
        Assert.Equal("Alice Cooper", detail.FullName);
        Assert.Single(detail.Applications);
        var app = detail.Applications[0];
        Assert.Equal("Applied", app.Status);
        Assert.Single(app.Feedbacks);
        Assert.Equal("Excellent skills", app.Feedbacks[0].Notes);
        Assert.Equal(5, app.Feedbacks[0].Score);
    }
}
