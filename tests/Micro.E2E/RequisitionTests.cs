using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Micro.E2E;

[Trait("Layer", "e2e")]
public class RequisitionTests : PageTest
{
    private const string BaseUrl = "http://localhost:4200";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        // Prerequisite: Login as Admin
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", "admin@microats.com");
        await Page.FillAsync("#password", "AdminPassword123!");
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/admin/requisitions"));
    }

    [Fact]
    public async Task AC01_CreateRequisition()
    {
        string title = $"Software Engineer {Guid.NewGuid()}";
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/admin/requisitions");
        await Page.ClickAsync("button:has-text('New Requisition')");

        // Act
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Engineering");
        await Page.FillAsync("#openings", "2");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        // Assert
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/admin/requisitions");
        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await Expect(row).ToBeVisibleAsync();
        await Expect(row.Locator(".status-badge")).ToHaveTextAsync(new System.Text.RegularExpressions.Regex("Draft"));
    }

    [Fact]
    public async Task AC02_UpdateDraftRequisition()
    {
        string title = $"To Be Updated {Guid.NewGuid()}";
        string updatedTitle = $"Senior {title}";
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/admin/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Test");
        await Page.FillAsync("#openings", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await row.Locator(".action-link:has-text('Edit')").ClickAsync();

        // Act
        await Page.FillAsync("#title", updatedTitle);
        await Page.ClickAsync("button:has-text('Update Requisition')");

        // Assert
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/admin/requisitions");
        var updatedRow = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = updatedTitle });
        await Expect(updatedRow).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AC03_FinalizeRequisition()
    {
        string title = $"To Be Finalized {Guid.NewGuid()}";
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/admin/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Test");
        await Page.FillAsync("#openings", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });

        // Act
        void HandleDialog(object? sender, IDialog dialog) => dialog.AcceptAsync();
        Page.Dialog += HandleDialog;
        try 
        {
            await row.Locator(".action-btn.finalize").ClickAsync();
        }
        finally
        {
            Page.Dialog -= HandleDialog;
        }

        // Assert
        await Expect(row.Locator(".status-badge")).ToHaveTextAsync(new System.Text.RegularExpressions.Regex("Finalized"));
        await Expect(row.Locator(".action-link:has-text('Edit')")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task AC04_CloseRequisition()
    {
        string title = $"To Be Closed {Guid.NewGuid()}";
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/admin/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Test");
        await Page.FillAsync("#openings", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });

        void HandleDialog(object? sender, IDialog dialog) => dialog.AcceptAsync();
        Page.Dialog += HandleDialog;
        try 
        {
            await row.Locator(".action-btn.finalize").ClickAsync();
            await row.Locator(".action-btn.close").ClickAsync();
        }
        finally
        {
            Page.Dialog -= HandleDialog;
        }

        // Act & Assert
        await Expect(row.Locator(".status-badge")).ToHaveTextAsync(new System.Text.RegularExpressions.Regex("Closed"));
    }

    [Fact]
    public async Task AC06_DraftOnlyEdits()
    {
        string title = $"Read Only Test {Guid.NewGuid()}";
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/admin/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Test");
        await Page.FillAsync("#openings", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");
        
        var row = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        
        void HandleDialog(object? sender, IDialog dialog) => dialog.AcceptAsync();
        Page.Dialog += HandleDialog;
        try 
        {
            await row.Locator(".action-btn.finalize").ClickAsync();
        }
        finally
        {
            Page.Dialog -= HandleDialog;
        }

        // Assert
        await Expect(row.Locator(".action-link:has-text('Edit')")).Not.ToBeVisibleAsync();
    }
}
