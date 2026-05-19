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
        await Page.FillAsync("#openings", "2");
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
        await Page.FillAsync("#openings", "1");
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
        await Page.FillAsync("#openings", "1");
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
        await Page.FillAsync("#openings", "1");
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
        await Page.FillAsync("#openings", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");
        
        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        
        // Act
        await row.Locator(".action-btn.finalize").ClickAsync();
        await Page.ClickAsync("button:has-text('Finalize')");

        // Assert
        await Expect(row.Locator(".action-link:has-text('Edit')")).Not.ToBeVisibleAsync();
    }
}
