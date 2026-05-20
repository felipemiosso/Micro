using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Endpoints.Application;
using Micro.API.Endpoints.JobPosting;
using Micro.API.Endpoints.Requisition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class ApplicationTests : IntegrationTestBase
{
    public ApplicationTests(TestDatabaseFixture fixture) : base(fixture)
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

    private async Task<Guid> CreatePublishedJob()
    {
        await AuthenticateAsync();
        var (deptId, bandId, ccId) = await GetLookupIds();
        var createRequest = new CreateRequisitionRequest(
            "Tester", deptId, bandId, ccId, 1,
            EmploymentType.FullTime, WorkplaceType.OnSite, "London", "Desc", false, null);
            
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest, JsonOptions);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);
        
        var adminJobsResponse = await Client.GetAsync("/api/jobs/admin");
        var adminJobs = await adminJobsResponse.Content.ReadFromJsonAsync<List<Micro.API.Data.Models.JobPosting>>(JsonOptions);
        return adminJobs!.First(j => j.RequisitionId == requisition.Id).Id;
    }

    [Fact]
    public async Task Apply_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var jobId = await CreatePublishedJob();
        
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("John Doe"), "name");
        content.Add(new StringContent("john@example.com"), "email");
        content.Add(new StringContent("123456789"), "phone");
        
        var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }); // %PDF-
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "resume", "resume.pdf");

        // Act
        var response = await Client.PostAsync($"/api/public/jobs/{jobId}/apply", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await AuthenticateAsync();
        var adminResponse = await Client.GetAsync($"/api/applications?jobPostingId={jobId}");
        var applications = await adminResponse.Content.ReadFromJsonAsync<List<JsonElement>>(JsonOptions);
        Assert.Single(applications!);
        
        // Regression test: Ensure CandidateId is present and valid
        var candidateId = applications![0].GetProperty("candidateId").GetGuid();
        Assert.NotEqual(Guid.Empty, candidateId);
    }

    [Fact]
    public async Task Apply_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var jobId = await CreatePublishedJob();
        
        var apply = async () => {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("John Doe"), "name");
            content.Add(new StringContent("duplicate@example.com"), "email");
            var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 });
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "resume", "resume.pdf");
            return await Client.PostAsync($"/api/public/jobs/{jobId}/apply", content);
        };

        await apply();

        // Act
        var response = await apply();

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Apply_NonPdfFile_ReturnsBadRequest()
    {
        // Arrange
        var jobId = await CreatePublishedJob();
        
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("John Doe"), "name");
        content.Add(new StringContent("john@example.com"), "email");
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "resume", "image.jpg");

        // Act
        var response = await Client.PostAsync($"/api/public/jobs/{jobId}/apply", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Apply_ClosedJob_ReturnsNotFound()
    {
        // Arrange
        var jobId = await CreatePublishedJob();
        await AuthenticateAsync();
        await Client.PostAsync($"/api/jobs/{jobId}/close", null);
        
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("John Doe"), "name");
        content.Add(new StringContent("john@example.com"), "email");
        var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "resume", "resume.pdf");

        // Act
        var response = await Client.PostAsync($"/api/public/jobs/{jobId}/apply", content);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Apply_SameCandidateDifferentJobs_ReturnsCreated()
    {
        // Arrange
        var job1Id = await CreatePublishedJob();
        var job2Id = await CreatePublishedJob();
        
        var apply = async (Guid jobId) => {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("John Doe"), "name");
            content.Add(new StringContent("john@example.com"), "email");
            var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 });
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "resume", "resume.pdf");
            return await Client.PostAsync($"/api/public/jobs/{jobId}/apply", content);
        };

        // Act
        var res1 = await apply(job1Id);
        var res2 = await apply(job2Id);

        // Assert
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, res2.StatusCode);
    }

    [Fact]
    public async Task GetResume_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync($"/api/admin/applications/{Guid.NewGuid()}/resume");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetResume_ValidRequest_ReturnsFile()
    {
        // Arrange
        var jobId = await CreatePublishedJob();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("File Tester"), "name");
        content.Add(new StringContent("file@example.com"), "email");
        var fileBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "resume", "resume.pdf");
        
        var createResponse = await Client.PostAsync($"/api/public/jobs/{jobId}/apply", content);
        var appJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Guid appId = appJson.GetProperty("id").GetGuid();

        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync($"/api/applications/{appId}/resume");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        var downloadedBytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(fileBytes, downloadedBytes);
    }

    [Fact]
    public async Task UpdateStatus_ValidRequest_UpdatesDatabase()
    {
        // Arrange
        var jobId = await CreatePublishedJob();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Status Guy"), "name");
        content.Add(new StringContent("status@example.com"), "email");
        content.Add(new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }), "resume", "resume.pdf");
        var createResponse = await Client.PostAsync($"/api/public/jobs/{jobId}/apply", content);
        var appJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Guid appId = appJson.GetProperty("id").GetGuid();

        await AuthenticateAsync();

        // Act
        var updateRequest = new UpdateStatusRequest(ApplicationStatus.Interview, ArchivalResolution.None);
        var response = await Client.PutAsJsonAsync($"/api/applications/{appId}/status", updateRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var detailResponse = await Client.GetAsync($"/api/applications/{appId}");
        var detail = await detailResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(ApplicationStatus.Interview.ToString(), detail.GetProperty("status").GetString());
    }

    [Fact]
    public async Task UpdateStatus_ArchiveWithoutResolution_ReturnsBadRequest()
    {
        // Arrange
        var jobId = await CreatePublishedJob();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Archive Fail"), "name");
        content.Add(new StringContent("archivefail@example.com"), "email");
        content.Add(new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }), "resume", "resume.pdf");
        var createResponse = await Client.PostAsync($"/api/public/jobs/{jobId}/apply", content);
        var appJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Guid appId = appJson.GetProperty("id").GetGuid();

        await AuthenticateAsync();

        // Act - Move to Archive without resolution
        var updateRequest = new UpdateStatusRequest(ApplicationStatus.Archive, ArchivalResolution.None);
        var response = await Client.PutAsJsonAsync($"/api/applications/{appId}/status", updateRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddFeedback_ValidRequest_PersistsFeedback()
    {
        // Arrange
        var jobId = await CreatePublishedJob();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Feedback Guy"), "name");
        content.Add(new StringContent("feedback@example.com"), "email");
        content.Add(new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }), "resume", "resume.pdf");
        var createResponse = await Client.PostAsync($"/api/public/jobs/{jobId}/apply", content);
        var appJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Guid appId = appJson.GetProperty("id").GetGuid();

        await AuthenticateAsync();

        // Act
        var feedbackRequest = new AddFeedbackRequest("Great candidate!", 5);
        var response = await Client.PostAsJsonAsync($"/api/applications/{appId}/feedback", feedbackRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var detailResponse = await Client.GetAsync($"/api/applications/{appId}");
        var detail = await detailResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var feedbacks = detail.GetProperty("feedbacks");
        Assert.Equal(1, feedbacks.GetArrayLength());
        Assert.Equal("Great candidate!", feedbacks[0].GetProperty("notes").GetString());
        Assert.Equal(5, feedbacks[0].GetProperty("score").GetInt32());
    }
}
