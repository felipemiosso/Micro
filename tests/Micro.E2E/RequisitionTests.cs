using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Micro.E2E;

[Trait("Layer", "e2e")]
[Collection("E2E Tests")]
public class RequisitionTests : PageTest
{
    private const string BaseUrl = "http://localhost:4200";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        // Seed user in Firebase Emulator
        string email = $"admin-{Guid.NewGuid()}@microats.com";
        await AuthSeedHelper.SeedAdminUserAsync(email);
        
        // Prerequisite: Login as Admin
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", email);
        await Page.FillAsync("#password", AuthSeedHelper.AdminPassword);
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/requisitions"));
    }

    [Fact]
    public async Task AC01_CreateRequisition()
    {
        string title = $"Software Engineer {Guid.NewGuid()}";
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/requisitions");
        await Page.ClickAsync("button:has-text('New Requisition')");

        // Act
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Engineering");
        await Page.FillAsync("#openingsCount", "2");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        // Assert
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/requisitions");
        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await Expect(row).ToBeVisibleAsync();
        await Expect(row.Locator(".badge-status")).ToHaveTextAsync(new System.Text.RegularExpressions.Regex("Draft"));
    }

    [Fact]
    public async Task AC02_UpdateDraftRequisition()
    {
        string title = $"To Be Updated {Guid.NewGuid()}";
        string updatedTitle = $"Senior {title}";
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Test");
        await Page.FillAsync("#openingsCount", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await row.Locator(".action-link:has-text('Edit')").ClickAsync();

        // Wait for the API to populate the form to prevent patchValue from overwriting our input
        await Expect(Page.Locator("#title")).ToHaveValueAsync(title);

        // Act
        await Page.FillAsync("#title", updatedTitle);
        await Page.ClickAsync("button:has-text('Update Requisition')");

        // Assert
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/requisitions");
        var updatedRow = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = updatedTitle });
        await Expect(updatedRow).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Fact]
    public async Task AC03_FinalizeRequisition()
    {
        string title = $"To Be Finalized {Guid.NewGuid()}";
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Test");
        await Page.FillAsync("#openingsCount", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });

        // Act
        await row.Locator(".action-btn.finalize").ClickAsync();
        await Page.ClickAsync("button:has-text('Finalize')");

        // Assert
        await Expect(row.Locator(".badge-status")).ToHaveTextAsync(new System.Text.RegularExpressions.Regex("Finalized"));
        await Expect(row.Locator(".action-link:has-text('Edit')")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task AC04_CloseRequisition()
    {
        string title = $"To Be Closed {Guid.NewGuid()}";
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Test");
        await Page.FillAsync("#openingsCount", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });

        // Act
        await row.Locator(".action-btn.finalize").ClickAsync();
        await Page.ClickAsync("button:has-text('Finalize')");
        
        await row.Locator(".action-btn.close").ClickAsync();
        await Page.ClickAsync("button:has-text('Close Requisition')");

        // Assert
        await Expect(row.Locator(".badge-status")).ToHaveTextAsync(new System.Text.RegularExpressions.Regex("Closed"));
    }

    [Fact]
    public async Task AC06_DraftOnlyEdits()
    {
        string title = $"Read Only Test {Guid.NewGuid()}";
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Test");
        await Page.FillAsync("#openingsCount", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");
        
        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        
        // Act
        await row.Locator(".action-btn.finalize").ClickAsync();
        await Page.ClickAsync("button:has-text('Finalize')");

        // Assert
        await Expect(row.Locator(".action-link:has-text('Edit')")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task AC016_01_ListDisplaysFilledOpeningsRatio()
    {
        string title = $"Ratio Test {Guid.NewGuid()}";
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Test");
        await Page.FillAsync("#openingsCount", "2");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        // Wait for list
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/requisitions");
        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await Expect(row).ToBeVisibleAsync();

        // Draft shows "2 (Draft)"
        await Expect(row.Locator("td").Nth(2)).ToHaveTextAsync("2 (Draft)");

        // Finalize
        await row.Locator(".action-btn.finalize").ClickAsync();
        await Page.ClickAsync("button:has-text('Finalize')");

        // Finalized shows "0 / 2"
        await Expect(row.Locator("td").Nth(2)).ToHaveTextAsync("0 / 2");
    }

    [Fact]
    public async Task AC016_02_03_04_FinalizedOpeningsManagement()
    {
        string title = $"Openings Management {Guid.NewGuid()}";
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Test");
        await Page.FillAsync("#openingsCount", "2");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await row.Locator(".action-btn.finalize").ClickAsync();
        await Page.ClickAsync("button:has-text('Finalize')");

        // Navigate to details
        await row.Locator("a[matTooltip='View Details']").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex($"/requisitions/edit/"));

        // AC-02: Finalized/Closed Requisition displays read-only list of openings
        var openingsTable = Page.Locator("text=Openings Tracking").Locator("..").Locator("table");
        await Expect(openingsTable).ToBeVisibleAsync();
        await Expect(openingsTable.Locator("tbody tr")).ToHaveCountAsync(2);
        
        // AC-03: Edit target start date of unfilled opening
        var firstRow = openingsTable.Locator("tbody tr").First;
        await firstRow.Locator("input[type='date']").FillAsync("2026-12-01");
        await firstRow.Locator("button[matTooltip='Save Date']").ClickAsync();
        
        // Wait for success toast
        await Expect(Page.Locator(".toast-success")).ToBeVisibleAsync();
        await Expect(firstRow.Locator("input[type='date']")).ToHaveValueAsync("2026-12-01");

        // AC-04: Cancel unfilled opening
        var secondRow = openingsTable.Locator("tbody tr").Nth(1);
        await secondRow.Locator("button[matTooltip='Cancel Opening']").ClickAsync();
        await Page.ClickAsync("button:has-text('Cancel Opening')");
        
        // Status changes to Cancelled
        await Expect(secondRow.Locator(".badge-status")).ToContainTextAsync("Cancelled");
        await Expect(secondRow.Locator("button[matTooltip='Cancel Opening']")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task AC016_05_DraftCanSetSeparateTargetDates()
    {
        string title = $"Multi Openings {Guid.NewGuid()}";
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Engineering");
        await Page.FillAsync("#openingsCount", "2");

        // The UI should display the individual date inputs
        var datesSection = Page.Locator("text=Individual Opening Target Dates").Locator("..");
        await Expect(datesSection).ToBeVisibleAsync();
        
        var dateInputs = datesSection.Locator("input[type='date']");
        await Expect(dateInputs).ToHaveCountAsync(2);

        await dateInputs.First.FillAsync("2026-10-01");
        await dateInputs.Nth(1).FillAsync("2026-11-01");

        await Page.ClickAsync("button:has-text('Create Requisition')");

        // Verify it saved
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/requisitions");
        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await row.Locator(".action-link:has-text('Edit')").ClickAsync();

        var detailDatesSection = Page.Locator("text=Individual Opening Target Dates").Locator("..");
        var detailDateInputs = detailDatesSection.Locator("input[type='date']");
        await Expect(detailDateInputs.First).ToHaveValueAsync("2026-10-01");
        await Expect(detailDateInputs.Nth(1)).ToHaveValueAsync("2026-11-01");
    }
}
