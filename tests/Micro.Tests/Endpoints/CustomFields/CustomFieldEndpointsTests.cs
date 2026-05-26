using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Endpoints.CustomFields;
using Micro.API.Endpoints.Requisition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Endpoints.CustomFields;

[Collection("TestDatabase")]
public class CustomFieldEndpointsTests : IntegrationTestBase
{
    public CustomFieldEndpointsTests(TestDatabaseFixture fixture) : base(fixture)
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
    public async Task ManageSelectableCustomFields_FullFlow()
    {
        // =========================================================================
        // Arrange
        // =========================================================================
        await AuthenticateAsync($"admin-{Guid.NewGuid()}@microats.com", roles: new Dictionary<string, object> { ["role"] = "Admin" });
        var (deptId, bandId, ccId) = await GetLookupIds();

        // 1. Create selectable custom field
        var createRequest = new CreateCustomFieldRequest(
            CustomFieldTargetEntity.Requisition,
            CustomFieldType.ShortText,
            "Requisition Option Code",
            "Optional code for requisition",
            false, // IsRequired
            false, // IsCandidateFacing
            null,
            false // IsGlobal = false
        );

        // =========================================================================
        // Act & Assert
        // =========================================================================

        // --- Step 1: Create definition ---
        var createResponse = await Client.PostAsJsonAsync("/api/custom-fields", createRequest, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var definition = await createResponse.Content.ReadFromJsonAsync<CustomFieldDefinitionDto>(JsonOptions);
        Assert.NotNull(definition);
        Assert.False(definition.IsGlobal);

        // --- Step 2: Verify in Selectable Pool ---
        var selectableResponse = await Client.GetAsync("/api/custom-fields/selectable?targetEntity=Requisition");
        Assert.Equal(HttpStatusCode.OK, selectableResponse.StatusCode);
        var selectableList = await selectableResponse.Content.ReadFromJsonAsync<List<CustomFieldDefinitionDto>>(JsonOptions);
        Assert.NotNull(selectableList);
        Assert.Contains(selectableList, d => d.Id == definition.Id);

        // --- Step 3: Create Requisition ---
        var createReqRequest = new CreateRequisitionRequest(
            "Test Requisition for Custom Fields", deptId, bandId, ccId, 1, 
            EmploymentType.FullTime, WorkplaceType.OnSite, 
            "London", "Desc", false, null);
        var reqResponse = await Client.PostAsJsonAsync("/api/requisitions", createReqRequest, JsonOptions);
        var requisition = await reqResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        Assert.NotNull(requisition);

        // --- Step 4: Verify selectable field not yet visible on requisition definitions query ---
        var generalDefsResponse = await Client.GetAsync("/api/custom-fields?entity=Requisition");
        var generalDefs = await generalDefsResponse.Content.ReadFromJsonAsync<List<CustomFieldDefinitionDto>>(JsonOptions);
        Assert.NotNull(generalDefs);
        Assert.DoesNotContain(generalDefs, d => d.Id == definition.Id);

        var initialDefsResponse = await Client.GetAsync($"/api/custom-fields?entity=Requisition&requisitionId={requisition.Id}");
        var initialDefs = await initialDefsResponse.Content.ReadFromJsonAsync<List<CustomFieldDefinitionDto>>(JsonOptions);
        Assert.NotNull(initialDefs);
        Assert.DoesNotContain(initialDefs, d => d.Id == definition.Id);

        // --- Step 5: Link selectable field to Requisition ---
        var linkResponse = await Client.PostAsJsonAsync($"/api/requisitions/{requisition.Id}/custom-fields", new LinkCustomFieldRequest(definition.Id), JsonOptions);
        Assert.Equal(HttpStatusCode.NoContent, linkResponse.StatusCode);

        // --- Step 6: Verify selectable field now visible on requisition definitions query ---
        var updatedDefsResponse = await Client.GetAsync($"/api/custom-fields?entity=Requisition&requisitionId={requisition.Id}");
        var updatedDefs = await updatedDefsResponse.Content.ReadFromJsonAsync<List<CustomFieldDefinitionDto>>(JsonOptions);
        Assert.NotNull(updatedDefs);
        Assert.Contains(updatedDefs, d => d.Id == definition.Id);

        // --- Step 7: Update Requisition with custom field value ---
        var updateRequest = new UpdateRequisitionRequest(
            "Test Requisition for Custom Fields", deptId, bandId, ccId, 1,
            EmploymentType.FullTime, WorkplaceType.OnSite,
            "London", "Desc", false, null,
            new List<RequisitionOpeningDto> { new RequisitionOpeningDto(1, null) },
            new List<CustomFieldValueInput> { new CustomFieldValueInput(definition.Id, "OPT-1234") });
        var updateResponse = await Client.PutAsJsonAsync($"/api/requisitions/{requisition.Id}", updateRequest, JsonOptions);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // --- Step 8: Unlinking with values should be blocked (Conflict 409) ---
        var failedUnlinkResponse = await Client.DeleteAsync($"/api/requisitions/{requisition.Id}/custom-fields/{definition.Id}");
        Assert.Equal(HttpStatusCode.Conflict, failedUnlinkResponse.StatusCode);

        // --- Step 9: Clear values and unlink successfully ---
        var clearValueRequest = new UpdateRequisitionRequest(
            "Test Requisition for Custom Fields", deptId, bandId, ccId, 1,
            EmploymentType.FullTime, WorkplaceType.OnSite,
            "London", "Desc", false, null,
            new List<RequisitionOpeningDto> { new RequisitionOpeningDto(1, null) },
            new List<CustomFieldValueInput> { new CustomFieldValueInput(definition.Id, null) }); // Clears value
        await Client.PutAsJsonAsync($"/api/requisitions/{requisition.Id}", clearValueRequest, JsonOptions);

        var unlinkResponse = await Client.DeleteAsync($"/api/requisitions/{requisition.Id}/custom-fields/{definition.Id}");
        Assert.Equal(HttpStatusCode.NoContent, unlinkResponse.StatusCode);

        // --- Step 10: Clean up definition ---
        var deleteResponse = await Client.DeleteAsync($"/api/custom-fields/{definition.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task CreateRequisition_WithSelectableCustomFields_LinksAndSavesValues()
    {
        // Arrange
        await AuthenticateAsync($"admin-{Guid.NewGuid()}@microats.com", roles: new Dictionary<string, object> { ["role"] = "Admin" });
        var (deptId, bandId, ccId) = await GetLookupIds();

        // 1. Create selectable custom field definition
        var createRequest = new CreateCustomFieldRequest(
            CustomFieldTargetEntity.Requisition,
            CustomFieldType.ShortText,
            "Create-Time Option Code",
            "Optional code on creation",
            false, // IsRequired
            false, // IsCandidateFacing
            null,
            false // IsGlobal = false
        );

        var createResponse = await Client.PostAsJsonAsync("/api/custom-fields", createRequest, JsonOptions);
        var definition = await createResponse.Content.ReadFromJsonAsync<CustomFieldDefinitionDto>(JsonOptions);
        Assert.NotNull(definition);

        // 2. Create Requisition with the custom field linked and value filled
        var createReqRequest = new CreateRequisitionRequest(
            "Test Creation Linkage Requisition", deptId, bandId, ccId, 1,
            EmploymentType.FullTime, WorkplaceType.OnSite,
            "New York", "Desc", false, null,
            new List<RequisitionOpeningDto> { new RequisitionOpeningDto(1, null) },
            new List<CustomFieldValueInput> { new CustomFieldValueInput(definition.Id, "NEW-VAL-123") },
            new List<Guid> { definition.Id } // LinkedCustomFieldIds
        );

        // Act
        var reqResponse = await Client.PostAsJsonAsync("/api/requisitions", createReqRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, reqResponse.StatusCode);
        var requisition = await reqResponse.Content.ReadFromJsonAsync<Requisition>(JsonOptions);
        Assert.NotNull(requisition);

        // Verify that selectable field is linked and its value is saved
        var valuesResponse = await Client.GetAsync($"/api/requisitions/{requisition.Id}");
        Assert.Equal(HttpStatusCode.OK, valuesResponse.StatusCode);

        var responseBody = await valuesResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(JsonOptions);
        var customFieldsArray = responseBody.GetProperty("customFields");
        Assert.Equal(System.Text.Json.JsonValueKind.Array, customFieldsArray.ValueKind);

        bool foundAndCorrect = false;
        foreach (var field in customFieldsArray.EnumerateArray())
        {
            if (field.GetProperty("definitionId").GetGuid() == definition.Id)
            {
                Assert.Equal("NEW-VAL-123", field.GetProperty("value").GetString());
                foundAndCorrect = true;
            }
        }
        Assert.True(foundAndCorrect);
    }

    [Fact]
    public async Task UpdateCustomFieldDefinition_Lifecycle()
    {
        // Arrange
        await AuthenticateAsync($"admin-{Guid.NewGuid()}@microats.com", roles: new Dictionary<string, object> { ["role"] = "Admin" });

        // 1. Create selectable custom field definition
        var createRequest = new CreateCustomFieldRequest(
            CustomFieldTargetEntity.Requisition,
            CustomFieldType.ShortText,
            "Initial Label",
            "Initial HelpText",
            false, // IsRequired
            false, // IsCandidateFacing
            null,
            false // IsGlobal = false
        );

        var createResponse = await Client.PostAsJsonAsync("/api/custom-fields", createRequest, JsonOptions);
        var definition = await createResponse.Content.ReadFromJsonAsync<CustomFieldDefinitionDto>(JsonOptions);
        Assert.NotNull(definition);
        Assert.False(definition.IsGlobal);

        // 2. Update without values (changing IsGlobal to true is allowed)
        var updateRequest1 = new UpdateCustomFieldRequest(
            "Updated Label",
            "Updated HelpText",
            true, // IsRequired = true
            false,
            null,
            true // IsGlobal = true
        );

        var updateResponse1 = await Client.PutAsJsonAsync($"/api/custom-fields/{definition.Id}", updateRequest1, JsonOptions);
        Assert.Equal(HttpStatusCode.OK, updateResponse1.StatusCode);
        var updated1 = await updateResponse1.Content.ReadFromJsonAsync<CustomFieldDefinitionDto>(JsonOptions);
        Assert.NotNull(updated1);
        Assert.True(updated1.IsGlobal);
        Assert.Equal("Updated Label", updated1.Label);

        // 3. Update back to IsGlobal = false (still no values)
        var updateRequest2 = new UpdateCustomFieldRequest(
            "Updated Label",
            "Updated HelpText",
            true,
            false,
            null,
            false // IsGlobal = false
        );

        var updateResponse2 = await Client.PutAsJsonAsync($"/api/custom-fields/{definition.Id}", updateRequest2, JsonOptions);
        Assert.Equal(HttpStatusCode.OK, updateResponse2.StatusCode);
        var updated2 = await updateResponse2.Content.ReadFromJsonAsync<CustomFieldDefinitionDto>(JsonOptions);
        Assert.NotNull(updated2);
        Assert.False(updated2.IsGlobal);
    }
}
