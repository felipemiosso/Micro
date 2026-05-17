using System.Net;
using System.Net.Http.Json;
using Micro.API.Data.Models;
using Micro.API.Endpoints;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class JobPostingTests : IntegrationTestBase
{
    public JobPostingTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    private void Authenticate()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");
    }

    [Fact]
    public async Task FinalizeRequisition_CreatesJobPosting()
    {
        // Arrange
        Authenticate();
        var createRequest = new RequisitionEndpoints.CreateRequisitionRequest("Engineer", "IT", 1);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();

        // Act
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);

        // Assert
        var jobsResponse = await Client.GetAsync("/api/jobs");
        var jobs = await jobsResponse.Content.ReadFromJsonAsync<List<JobPostingEndpoints.PublicJobResponse>>();
        Assert.Contains(jobs!, j => j.Title == "Engineer");
    }

    [Fact]
    public async Task CloseRequisition_ClosesJobPosting()
    {
        // Arrange
        Authenticate();
        var createRequest = new RequisitionEndpoints.CreateRequisitionRequest("Closer", "IT", 1);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);

        // Act
        await Client.PostAsync($"/api/requisitions/{requisition.Id}/close", null);

        // Assert
        var jobsResponse = await Client.GetAsync("/api/jobs");
        var jobs = await jobsResponse.Content.ReadFromJsonAsync<List<JobPostingEndpoints.PublicJobResponse>>();
        Assert.DoesNotContain(jobs!, j => j.Title == "Closer");
    }

    [Fact]
    public async Task UpdateJobPosting_ReturnsNoContent()
    {
        // Arrange
        Authenticate();
        var createRequest = new RequisitionEndpoints.CreateRequisitionRequest("To Update", "IT", 1);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);
        
        var adminJobsResponse = await Client.GetAsync("/api/admin/jobs");
        var adminJobs = await adminJobsResponse.Content.ReadFromJsonAsync<List<JobPosting>>();
        var job = adminJobs!.First(j => j.RequisitionId == requisition.Id);

        var updateRequest = new JobPostingEndpoints.UpdateJobPostingRequest("New Title", "New Desc", "New Req");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/admin/jobs/{job.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var publicJobResponse = await Client.GetAsync($"/api/jobs/{job.Id}");
        var updatedJob = await publicJobResponse.Content.ReadFromJsonAsync<JobPostingEndpoints.PublicJobDetailResponse>();
        Assert.Equal("New Title", updatedJob!.Title);
    }
}
