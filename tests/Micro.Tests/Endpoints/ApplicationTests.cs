using System.Net;
using System.Net.Http.Json;
using Micro.API.Data.Models;
using Micro.API.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class ApplicationTests : IntegrationTestBase
{
    public ApplicationTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    private async Task AuthenticateAsync()
    {
        var loginRequest = new { Email = "admin@microats.com", Password = "AdminPassword123!" };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthTests.LoginResponse>();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);
    }

    private async Task<Guid> CreatePublishedJob()
    {
        await AuthenticateAsync();
        var createRequest = new RequisitionEndpoints.CreateRequisitionRequest("Tester", "QA", 1);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);
        
        var adminJobsResponse = await Client.GetAsync("/api/admin/jobs");
        var adminJobs = await adminJobsResponse.Content.ReadFromJsonAsync<List<JobPosting>>();
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
        var adminResponse = await Client.GetAsync($"/api/admin/applications?jobPostingId={jobId}");
        var applications = await adminResponse.Content.ReadFromJsonAsync<List<dynamic>>();
        Assert.Single(applications!);
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
        await Client.PostAsync($"/api/admin/jobs/{jobId}/close", null);
        
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
        var appInfo = await createResponse.Content.ReadFromJsonAsync<dynamic>();
        Guid appId = appInfo!.GetProperty("id").GetGuid();

        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync($"/api/admin/applications/{appId}/resume");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        var downloadedBytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(fileBytes, downloadedBytes);
    }
}
