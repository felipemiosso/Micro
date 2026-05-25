using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Endpoints.Requisition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class RequisitionTests : IntegrationTestBase
{
    public RequisitionTests(TestDatabaseFixture fixture) : base(fixture)
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
    public async Task Create_WithValidData_ReturnsCreated()
    {
        // Arrange
        var email = $"test-{Guid.NewGuid()}@microats.com";
        await AuthenticateAsync(email);
        var (deptId, bandId, ccId) = await GetLookupIds();
        
        var request = new CreateRequisitionRequest(
            "Dev", deptId, bandId, ccId, 2, 
            EmploymentType.FullTime, WorkplaceType.OnSite, 
            "London", "Job Desc", false, null);

        // Act
        var response = await Client.PostAsJsonAsync("/api/requisitions", request, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var requisition = await response.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        Assert.NotNull(requisition);
        Assert.Equal("Dev", requisition.Title);
        Assert.Equal(email, requisition.CreatedBy);
    }

    [Fact]
    public async Task Update_DraftRequisition_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var (deptId, bandId, ccId) = await GetLookupIds();
        
        var createRequest = new CreateRequisitionRequest(
            "Dev", deptId, bandId, ccId, 2, 
            EmploymentType.FullTime, WorkplaceType.OnSite, 
            "London", "Job Desc", false, null);
            
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest, JsonOptions);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        
        var updateRequest = new UpdateRequisitionRequest(
            "Senior Dev", deptId, bandId, ccId, 1,
            EmploymentType.FullTime, WorkplaceType.Remote,
            "Remote", "New Desc", false, null,
            new List<RequisitionOpeningDto> { new RequisitionOpeningDto(1, null) });

        // Act
        var response = await Client.PutAsJsonAsync($"/api/requisitions/{requisition!.Id}", updateRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_DraftRequisition_WithNullTargetStartDate_SavesNull()
    {
        // Arrange
        await AuthenticateAsync();
        var (deptId, bandId, ccId) = await GetLookupIds();
        
        var createRequest = new CreateRequisitionRequest(
            "Dev", deptId, bandId, ccId, 2, 
            EmploymentType.FullTime, WorkplaceType.OnSite, 
            "London", "Job Desc", false, DateTime.UtcNow);
            
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest, JsonOptions);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        
        var updateRequest = new UpdateRequisitionRequest(
            "Senior Dev", deptId, bandId, ccId, 1,
            EmploymentType.FullTime, WorkplaceType.Remote,
            "Remote", "New Desc", false, null,
            new List<RequisitionOpeningDto> { new RequisitionOpeningDto(1, null) });

        // Act
        var response = await Client.PutAsJsonAsync($"/api/requisitions/{requisition!.Id}", updateRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var getResponse = await Client.GetAsync("/api/requisitions");
        var requisitions = await getResponse.Content.ReadFromJsonAsync<List<Requisition>>(JsonOptions);
        var updated = requisitions!.First(r => r.Id == requisition.Id);
        Assert.Null(updated.TargetStartDate);
        Assert.Null(updated.Openings.First(o => o.SequenceNumber == 1).TargetStartDate);
    }

    [Fact]
    public async Task Finalize_DraftRequisition_ChangesStatus()
    {
        // Arrange
        await AuthenticateAsync();
        var (deptId, bandId, ccId) = await GetLookupIds();
        
        var createRequest = new CreateRequisitionRequest(
            "Dev", deptId, bandId, ccId, 2, 
            EmploymentType.FullTime, WorkplaceType.OnSite, 
            "London", "Job Desc", false, null);
            
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);

        // Act
        var response = await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var getResponse = await Client.GetAsync("/api/requisitions");
        var requisitions = await getResponse.Content.ReadFromJsonAsync<List<Requisition>>(JsonOptions);
        Assert.Equal(RequisitionStatus.Finalized, requisitions!.First(r => r.Id == requisition.Id).Status);
    }

    [Fact]
    public async Task Update_FinalizedRequisition_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var (deptId, bandId, ccId) = await GetLookupIds();
        
        var createRequest = new CreateRequisitionRequest(
            "Dev", deptId, bandId, ccId, 2, 
            EmploymentType.FullTime, WorkplaceType.OnSite, 
            "London", "Job Desc", false, null);
            
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);

        // Act
        var updateRequest = new UpdateRequisitionRequest(
            "Senior Dev", deptId, bandId, ccId, 1,
            EmploymentType.FullTime, WorkplaceType.Remote,
            "Remote", "New Desc", false, null);
            
        var response = await Client.PutAsJsonAsync($"/api/requisitions/{requisition!.Id}", updateRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Close_ExistingRequisition_ChangesStatus()
    {
        // Arrange
        await AuthenticateAsync();
        var (deptId, bandId, ccId) = await GetLookupIds();
        
        var createRequest = new CreateRequisitionRequest(
            "To Close", deptId, bandId, ccId, 1, 
            EmploymentType.FullTime, WorkplaceType.OnSite, 
            "London", "Job Desc", false, null);
            
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);

        // Act
        var response = await Client.PostAsync($"/api/requisitions/{requisition!.Id}/close", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync($"/api/requisitions/{requisition.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        Assert.Equal(RequisitionStatus.Closed, updated!.Status);
        Assert.NotNull(updated.ClosedAt);
    }

    [Fact]
    public async Task Close_NonExistentRequisition_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/api/requisitions/{nonExistentId}/close", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOpening_CancelOpening_WhenAllFilledOrCancelled_ClosesRequisitionAndJob()
    {
        // Arrange
        var email = $"test-{Guid.NewGuid()}@microats.com";
        await AuthenticateAsync(email);
        var (deptId, bandId, ccId) = await GetLookupIds();
        
        var createRequest = new CreateRequisitionRequest(
            "To Cancel", deptId, bandId, ccId, 1, 
            EmploymentType.FullTime, WorkplaceType.OnSite, 
            "London", "Job Desc", false, null);
            
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest, JsonOptions);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);
        
        var reqResponse = await Client.GetAsync($"/api/requisitions/{requisition.Id}");
        var finalizedReq = await reqResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        var openingId = finalizedReq!.Openings.First().Id;

        // Act
        var updateRequest = new Micro.API.Endpoints.Requisition.RequisitionEndpoints.UpdateRequisitionOpeningRequest(null, OpeningStatus.Cancelled);
        var response = await Client.PutAsJsonAsync($"/api/requisitions/{requisition.Id}/openings/{openingId}", updateRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var finalReqResponse = await Client.GetAsync($"/api/requisitions/{requisition.Id}");
        var closedReq = await finalReqResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        Assert.Equal(RequisitionStatus.Closed, closedReq!.Status);
        Assert.NotNull(closedReq.ClosedAt);

        var jobResponse = await Client.GetAsync($"/api/jobs/admin");
        var jobs = await jobResponse.Content.ReadFromJsonAsync<List<AdminJobTestResponse>>(JsonOptions);
        var jobDetail = jobs!.First(j => j.RequisitionId == requisition.Id);
        Assert.Equal(JobPostingStatus.Closed, jobDetail.Status);
    }
}
