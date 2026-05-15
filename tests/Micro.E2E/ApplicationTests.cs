using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Micro.E2E;

[Trait("Layer", "e2e")]
[Collection("E2E Tests")]
public class ApplicationTests : PageTest
{
    private const string BaseUrl = "http://localhost:4200";

    private async Task CreatePublishedJob(string title)
    {
        // Login
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", "admin@microats.com");
        await Page.FillAsync("#password", "AdminPassword123!");
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/admin/requisitions"));

        // Create Requisition
        await Page.GotoAsync($"{BaseUrl}/admin/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "QA");
        await Page.FillAsync("#openings", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        // Finalize
        var reqRow = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await Expect(reqRow).ToBeVisibleAsync(); // Wait for the row to appear

        void HandleDialog(object? sender, IDialog dialog) => dialog.AcceptAsync();
        Page.Dialog += HandleDialog;
        try { await reqRow.Locator(".action-btn.finalize:has-text('Publish')").ClickAsync(); } finally { Page.Dialog -= HandleDialog; }
        
        // Wait for finalize to complete (indicated by status change in row)
        await Expect(reqRow.Locator(".status-badge")).ToHaveTextAsync("Finalized");

        // Logout
        await Page.ClickAsync(".btn-logout");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/login"));
    }

    [Fact]
    public async Task AC01_SuccessfulApplicationSubmission()
    {
        string jobTitle = $"Public Job {Guid.NewGuid()}";
        await CreatePublishedJob(jobTitle);

        // Go to public board
        await Page.GotoAsync($"{BaseUrl}/jobs");
        var jobCard = Page.Locator(".job-card").Filter(new() { HasText = jobTitle });
        await jobCard.Locator(".btn-link").ClickAsync();

        // Click Apply
        await Page.ClickAsync("a:has-text('Apply Now')");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/apply"));

        // Fill form
        await Page.FillAsync("#name", "Jane Doe");
        await Page.FillAsync("#email", "jane@example.com");
        await Page.FillAsync("#phone", "555-0199");

        // Create dummy PDF
        var tempPath = Path.Combine(Path.GetTempPath(), "dummy.pdf");
        await File.WriteAllBytesAsync(tempPath, new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 });

        // Upload resume
        await Page.SetInputFilesAsync("#resume", tempPath);

        // Submit
        await Page.ClickAsync("button[type='submit']");

        // Success page
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/success"));
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Application Submitted!");
        
        File.Delete(tempPath);
    }
}
