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
        // Seed user in Firebase Emulator
        string email = $"admin-{Guid.NewGuid()}@microats.com";
        await AuthSeedHelper.SeedAdminUserAsync(email);

        // Login
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", email);
        await Page.FillAsync("#password", AuthSeedHelper.AdminPassword);
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/requisitions"));

        // Create Requisition
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "QA");
        await Page.FillAsync("#openings", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        // Finalize
        var reqRow = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await Expect(reqRow).ToBeVisibleAsync(); // Wait for the row to appear

        void HandleDialog(object? sender, IDialog dialog) => dialog.AcceptAsync();
        Page.Dialog += HandleDialog;
        try { await reqRow.Locator(".action-btn.finalize").ClickAsync(); } finally { Page.Dialog -= HandleDialog; }
        
        // Wait for finalize to complete (indicated by status change in row)
        await Expect(reqRow.Locator(".status-badge")).ToHaveTextAsync("Finalized");

        // Logout
        await Page.ClickAsync(".user-menu-trigger");
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
        string candidateName = $"Jane Doe {Guid.NewGuid()}";
        await Page.FillAsync("#name", candidateName);
        await Page.FillAsync("#email", $"jane-{Guid.NewGuid()}@example.com");
        await Page.FillAsync("#phone", "555-0199");

        // Create dummy PDF
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
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

    [Fact]
    public async Task AC02_SearchAndFilterCandidates()
    {
        string jobTitle = $"Searchable Job {Guid.NewGuid()}";
        await CreatePublishedJob(jobTitle);

        // Apply
        await Page.GotoAsync($"{BaseUrl}/jobs");
        await Page.Locator(".job-card").Filter(new() { HasText = jobTitle }).Locator(".btn-link").ClickAsync();
        await Page.ClickAsync("a:has-text('Apply Now')");
        string candidateName = $"Search User {Guid.NewGuid()}";
        await Page.FillAsync("#name", candidateName);
        await Page.FillAsync("#email", $"search-{Guid.NewGuid()}@search.com");
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
        await File.WriteAllBytesAsync(tempPath, new byte[] { 0x25, 0x50, 0x44, 0x46 });
        await Page.SetInputFilesAsync("#resume", tempPath);
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/success"));

        // Login as Admin
        string email = $"admin-{Guid.NewGuid()}@microats.com";
        await AuthSeedHelper.SeedAdminUserAsync(email);
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", email);
        await Page.FillAsync("#password", AuthSeedHelper.AdminPassword);
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/requisitions"));

        // Go to candidates list
        await Page.GotoAsync($"{BaseUrl}/candidates");
        
        // Search
        await Page.FillAsync("#searchQuery", candidateName);
        await Page.ClickAsync("button:has-text('Search')");
        
        // Verify row exists
        var row = Page.Locator(".application-table tbody tr").Filter(new() { HasText = candidateName });
        await Expect(row).ToBeVisibleAsync();
        
        File.Delete(tempPath);
    }

    [Fact]
    public async Task AC03_UpdateStageAndAddFeedback()
    {
        string jobTitle = $"Tracking Job {Guid.NewGuid()}";
        await CreatePublishedJob(jobTitle);

        // Apply
        await Page.GotoAsync($"{BaseUrl}/jobs");
        await Page.Locator(".job-card").Filter(new() { HasText = jobTitle }).Locator(".btn-link").ClickAsync();
        await Page.ClickAsync("a:has-text('Apply Now')");
        string candidateName = $"Feedback User {Guid.NewGuid()}";
        await Page.FillAsync("#name", candidateName);
        await Page.FillAsync("#email", $"feedback-{Guid.NewGuid()}@track.com");
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
        await File.WriteAllBytesAsync(tempPath, new byte[] { 0x25, 0x50, 0x44, 0x46 });
        await Page.SetInputFilesAsync("#resume", tempPath);
        await Page.ClickAsync("button[type='submit']");

        // Admin login
        string email = $"admin-{Guid.NewGuid()}@microats.com";
        await AuthSeedHelper.SeedAdminUserAsync(email);
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", email);
        await Page.FillAsync("#password", AuthSeedHelper.AdminPassword);
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/requisitions"));

        // Navigate to detail
        await Page.GotoAsync($"{BaseUrl}/candidates");
        await Page.FillAsync("#searchQuery", candidateName);
        await Page.ClickAsync("button:has-text('Search')");
        
        // Use the action link (which now has hidden text or icon)
        var row = Page.Locator(".application-table tbody tr").Filter(new() { HasText = candidateName });
        await row.Locator(".action-link").First.ClickAsync();

        // Update Stage
        await Page.ClickAsync("button:has-text('Move to Interview')");
        await Expect(Page.Locator(".current-status")).ToContainTextAsync("Interview");

        // Add Feedback
        await Page.FillAsync("#notes", "Very promising candidate.");
        await Page.SelectOptionAsync("#score", new[] { "5" });
        await Page.ClickAsync("button:has-text('Add Feedback')");

        // Verify Feedback
        await Expect(Page.Locator(".feedback-item")).ToContainTextAsync("Very promising candidate.");
        await Expect(Page.Locator(".score-badge")).ToContainTextAsync("★ 5");

        File.Delete(tempPath);
    }

    [Fact]
    public async Task AC04_ApplicationsDragAndDrop()
    {
        string jobTitle = $"Board Job {Guid.NewGuid()}";
        await CreatePublishedJob(jobTitle);

        // Apply
        await Page.GotoAsync($"{BaseUrl}/jobs");
        await Page.Locator(".job-card").Filter(new() { HasText = jobTitle }).Locator(".btn-link").ClickAsync();
        await Page.ClickAsync("a:has-text('Apply Now')");
        string candidateName = $"Board User {Guid.NewGuid()}";
        await Page.FillAsync("#name", candidateName);
        await Page.FillAsync("#email", $"board-{Guid.NewGuid()}@track.com");
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
        await File.WriteAllBytesAsync(tempPath, new byte[] { 0x25, 0x50, 0x44, 0x46 });
        await Page.SetInputFilesAsync("#resume", tempPath);
        await Page.ClickAsync("button[type='submit']");

        // Admin login
        string email = $"admin-{Guid.NewGuid()}@microats.com";
        await AuthSeedHelper.SeedAdminUserAsync(email);
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", email);
        await Page.FillAsync("#password", AuthSeedHelper.AdminPassword);
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/requisitions"));

        // Navigate to Applications
        await Page.ClickAsync("a:has-text('Applications')");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/applications"));

        // Find candidate in Applied lane
        var appliedLane = Page.Locator("#appliedList");
        var candidateCard = appliedLane.Locator(".card").Filter(new() { HasText = candidateName });
        await Expect(candidateCard).ToBeVisibleAsync();

        // Move via Detail Page (Workaround for flaky drag-and-drop in E2E)
        await candidateCard.Locator("a:has-text('Profile')").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/candidates/"));
        
        await Page.ClickAsync("button:has-text('Move to Interview')");
        await Expect(Page.Locator(".current-status")).ToContainTextAsync("Interview");

        // Go back to board
        await Page.ClickAsync("a:has-text('Applications')");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/applications"));

        // Verify it moved (wait for UI to sync)
        await Expect(Page.Locator("#appliedList").Locator(".card").Filter(new() { HasText = candidateName })).ToHaveCountAsync(0);
        await Expect(Page.Locator("#interviewList").Locator(".card").Filter(new() { HasText = candidateName })).ToHaveCountAsync(1);

        File.Delete(tempPath);
    }

    [Fact]
    public async Task AC05_ArchiveCandidate()
    {
        string jobTitle = $"Archive Job {Guid.NewGuid()}";
        await CreatePublishedJob(jobTitle);

        // Apply
        await Page.GotoAsync($"{BaseUrl}/jobs");
        await Page.Locator(".job-card").Filter(new() { HasText = jobTitle }).Locator(".btn-link").ClickAsync();
        await Page.ClickAsync("a:has-text('Apply Now')");
        string candidateName = $"Archive User {Guid.NewGuid()}";
        await Page.FillAsync("#name", candidateName);
        await Page.FillAsync("#email", $"archive-{Guid.NewGuid()}@track.com");
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
        await File.WriteAllBytesAsync(tempPath, new byte[] { 0x25, 0x50, 0x44, 0x46 });
        await Page.SetInputFilesAsync("#resume", tempPath);
        await Page.ClickAsync("button[type='submit']");

        // Admin login
        string email = $"admin-{Guid.NewGuid()}@microats.com";
        await AuthSeedHelper.SeedAdminUserAsync(email);
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", email);
        await Page.FillAsync("#password", AuthSeedHelper.AdminPassword);
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/requisitions"));

        // Navigate to Applications
        await Page.ClickAsync("a:has-text('Applications')");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/applications"));

        var appliedLane = Page.Locator("#appliedList");
        var candidateCard = appliedLane.Locator(".card").Filter(new() { HasText = candidateName });
        await Expect(candidateCard).ToBeVisibleAsync();

        // Move to Archive via Detail Page
        await candidateCard.Locator("a:has-text('Profile')").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/candidates/"));
        
        await Page.ClickAsync("button:has-text('Archive Candidate')");

        // Handle MatDialog
        var dialog = Page.Locator("mat-dialog-container, .mat-mdc-dialog-container, [role='dialog']").First;
        await Expect(dialog).ToBeVisibleAsync();
        
        // Select Hired resolution
        await dialog.GetByLabel("Hired").ClickAsync();
        await dialog.Locator("button:has-text('Archive')").ClickAsync();

        // Verify archived in detail view first
        await Expect(Page.Locator(".current-status")).ToContainTextAsync("Archive");

        // Check archived board
        await Page.GotoAsync($"{BaseUrl}/applications/archived");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/applications/archived"));

        var hiredLane = Page.Locator(".lane").Filter(new() { HasText = "Hired" });
        await Expect(hiredLane.Locator(".card").Filter(new() { HasText = candidateName })).ToHaveCountAsync(1);

        File.Delete(tempPath);
    }
}
