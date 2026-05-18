using System.Net;
using System.Net.Http.Json;
using Micro.API.Data.Models;
using Micro.API.Endpoints.Requisition;
using Xunit;

namespace Micro.Tests.Endpoints;

[Collection("TestDatabase")]
public class RequisitionTests : IntegrationTestBase
{
    public RequisitionTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        // Arrange
        var email = $"test-{Guid.NewGuid()}@microats.com";
        await AuthenticateAsync(email);
        var request = new CreateRequisitionRequest("Dev", "IT", 2);

        // Act
        var response = await Client.PostAsJsonAsync("/api/requisitions", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var requisition = await response.Content.ReadFromJsonAsync<Requisition>();
        Assert.NotNull(requisition);
        Assert.Equal("Dev", requisition.Title);
        Assert.Equal(email, requisition.CreatedBy);
    }

    [Fact]
    public async Task Update_DraftRequisition_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var createRequest = new CreateRequisitionRequest("Dev", "IT", 2);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();
        var updateRequest = new UpdateRequisitionRequest("Senior Dev", "IT", 1);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/requisitions/{requisition!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Finalize_DraftRequisition_ChangesStatus()
    {
        // Arrange
        await AuthenticateAsync();
        var createRequest = new CreateRequisitionRequest("Dev", "IT", 2);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();

        // Act
        var response = await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var getResponse = await Client.GetAsync("/api/requisitions");
        var requisitions = await getResponse.Content.ReadFromJsonAsync<List<Requisition>>();
        Assert.Equal(RequisitionStatus.Finalized, requisitions!.First(r => r.Id == requisition.Id).Status);
    }

    [Fact]
    public async Task Update_FinalizedRequisition_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var createRequest = new CreateRequisitionRequest("Dev", "IT", 2);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();
        await Client.PostAsync($"/api/requisitions/{requisition!.Id}/finalize", null);

        // Act
        var updateRequest = new UpdateRequisitionRequest("Senior Dev", "IT", 1);
        var response = await Client.PutAsJsonAsync($"/api/requisitions/{requisition!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Close_ExistingRequisition_ChangesStatus()
    {
        // Arrange
        await AuthenticateAsync();
        var createRequest = new CreateRequisitionRequest("To Close", "IT", 1);
        var createResponse = await Client.PostAsJsonAsync("/api/requisitions", createRequest);
        var requisition = await createResponse.Content.ReadFromJsonAsync<Requisition>();

        // Act
        var response = await Client.PostAsync($"/api/requisitions/{requisition!.Id}/close", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync($"/api/requisitions/{requisition.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<Requisition>();
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
}
