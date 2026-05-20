using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Endpoints.JobPosting;
using Micro.API.Endpoints.Requisition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class JobPostingTests : IntegrationTestBase
{
    public JobPostingTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    private async Task<(Guid deptId, Guid bandId, Guid ccId)> GetLookupIds()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
        var dept = await db.Departments.FirstAsync();
        var band = await db.SalaryBands.FirstAsync();
        var cc = await db.CostCenters.FirstAsync();
        return (dept.Id, band.Id, cc.Id);
    }

    [Fact]
    public async Task FinalizeRequisition_CreatesJobPosting()
    {
        // Arrange
        await AuthenticateAsync();
        var (deptId, bandId, ccId) = await GetLookupIds();
        var createRequest = new CreateRequisitionRequest(
            "Engineer", deptId, bandId, ccId, 1,
            EmploymentType.FullTime, WorkplaceType.OnSite, "London", "Desc", false, null);
            
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest, JsonOptions);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);

        // Act
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);

        // Assert
        var jobsResponse = await Client.GetAsync("/api/jobs");
        var jobs = await jobsResponse.Content.ReadFromJsonAsync<List<PublicJobResponse>>(JsonOptions);
        Assert.Contains(jobs!, j => j.Title == "Engineer");
    }

    [Fact]
    public async Task CloseRequisition_ClosesJobPosting()
    {
        // Arrange
        await AuthenticateAsync();
        var (deptId, bandId, ccId) = await GetLookupIds();
        var createRequest = new CreateRequisitionRequest(
            "Closer", deptId, bandId, ccId, 1,
            EmploymentType.FullTime, WorkplaceType.OnSite, "London", "Desc", false, null);
            
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest, JsonOptions);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);

        // Act
        await Client.PostAsync($"/api/requisitions/{requisition.Id}/close", null);

        // Assert
        var jobsResponse = await Client.GetAsync("/api/jobs");
        var jobs = await jobsResponse.Content.ReadFromJsonAsync<List<PublicJobResponse>>(JsonOptions);
        Assert.DoesNotContain(jobs!, j => j.Title == "Closer");
    }

    [Fact]
    public async Task UpdateJobPosting_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var (deptId, bandId, ccId) = await GetLookupIds();
        var createRequest = new CreateRequisitionRequest(
            "To Update", deptId, bandId, ccId, 1,
            EmploymentType.FullTime, WorkplaceType.OnSite, "London", "Desc", false, null);
            
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest, JsonOptions);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);
        
        var adminJobsResponse = await Client.GetAsync("/api/jobs/admin");
        var adminJobs = await adminJobsResponse.Content.ReadFromJsonAsync<List<Micro.API.Data.Models.JobPosting>>(JsonOptions);
        var job = adminJobs!.First(j => j.RequisitionId == requisition.Id);

        var updateRequest = new UpdateJobPostingRequest("New Title", "New Desc", "New Req");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/jobs/{job.Id}", updateRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var publicJobResponse = await Client.GetAsync($"/api/jobs/{job.Id}");
        var updatedJob = await publicJobResponse.Content.ReadFromJsonAsync<PublicJobDetailResponse>(JsonOptions);
        Assert.Equal("New Title", updatedJob!.Title);
    }
}
