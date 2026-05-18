using System.Net;
using System.Net.Http.Json;
using Micro.API.Data.Models;
using Micro.API.Endpoints.JobPosting;
using Micro.API.Endpoints.Requisition;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class JobPostingTests : IntegrationTestBase
{
    public JobPostingTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task FinalizeRequisition_CreatesJobPosting()
    {
        // Arrange
        await AuthenticateAsync();
        var createRequest = new CreateRequisitionRequest("Engineer", "IT", 1);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();

        // Act
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);

        // Assert
        var jobsResponse = await Client.GetAsync("/api/jobs");
        var jobs = await jobsResponse.Content.ReadFromJsonAsync<List<PublicJobResponse>>();
        Assert.Contains(jobs!, j => j.Title == "Engineer");
    }

    [Fact]
    public async Task CloseRequisition_ClosesJobPosting()
    {
        // Arrange
        await AuthenticateAsync();
        var createRequest = new CreateRequisitionRequest("Closer", "IT", 1);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);

        // Act
        await Client.PostAsync($"/api/requisitions/{requisition.Id}/close", null);

        // Assert
        var jobsResponse = await Client.GetAsync("/api/jobs");
        var jobs = await jobsResponse.Content.ReadFromJsonAsync<List<PublicJobResponse>>();
        Assert.DoesNotContain(jobs!, j => j.Title == "Closer");
    }

    [Fact]
    public async Task UpdateJobPosting_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var createRequest = new CreateRequisitionRequest("To Update", "IT", 1);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);
        
        var adminJobsResponse = await Client.GetAsync("/api/admin/jobs");
        var adminJobs = await adminJobsResponse.Content.ReadFromJsonAsync<List<Micro.API.Data.Models.JobPosting>>();
        var job = adminJobs!.First(j => j.RequisitionId == requisition.Id);

        var updateRequest = new UpdateJobPostingRequest("New Title", "New Desc", "New Req");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/admin/jobs/{job.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var publicJobResponse = await Client.GetAsync($"/api/jobs/{job.Id}");
        var updatedJob = await publicJobResponse.Content.ReadFromJsonAsync<PublicJobDetailResponse>();
        Assert.Equal("New Title", updatedJob!.Title);
    }
}
