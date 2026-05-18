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

    [Fact]
    public async Task AC02_SearchAndFilterCandidates()
    {
        string jobTitle = $"Searchable Job {Guid.NewGuid()}";
        await CreatePublishedJob(jobTitle);

        // Apply
        await Page.GotoAsync($"{BaseUrl}/jobs");
        await Page.Locator(".job-card").Filter(new() { HasText = jobTitle }).Locator(".btn-link").ClickAsync();
        await Page.ClickAsync("a:has-text('Apply Now')");
        string candidateName = $"Unique Candidate {Guid.NewGuid()}";
        await Page.FillAsync("#name", candidateName);
        await Page.FillAsync("#email", "unique@search.com");
        var tempPath = Path.Combine(Path.GetTempPath(), "resume.pdf");
        await File.WriteAllBytesAsync(tempPath, new byte[] { 0x25, 0x50, 0x44, 0x46 });
        await Page.SetInputFilesAsync("#resume", tempPath);
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/success"));

        // Login as Admin
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", "admin@microats.com");
        await Page.FillAsync("#password", "AdminPassword123!");
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/admin/requisitions"));

        // Go to candidates list
        await Page.GotoAsync($"{BaseUrl}/admin/candidates");
        
        // Search
        await Page.FillAsync("#searchQuery", candidateName);
        await Page.ClickAsync("button:has-text('Search')");
        
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
        await Page.FillAsync("#email", "feedback@track.com");
        var tempPath = Path.Combine(Path.GetTempPath(), "resume.pdf");
        await File.WriteAllBytesAsync(tempPath, new byte[] { 0x25, 0x50, 0x44, 0x46 });
        await Page.SetInputFilesAsync("#resume", tempPath);
        await Page.ClickAsync("button[type='submit']");

        // Admin login
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", "admin@microats.com");
        await Page.FillAsync("#password", "AdminPassword123!");
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/admin/requisitions"));

        // Navigate to detail
        await Page.GotoAsync($"{BaseUrl}/admin/candidates");
        await Page.FillAsync("#searchQuery", candidateName);
        await Page.ClickAsync("button:has-text('Search')");
        await Page.ClickAsync(".action-link:has-text('View Details')");

        // Update Stage
        await Page.ClickAsync("button:has-text('Interview')");
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
        string jobTitle = $"Applications Job {Guid.NewGuid()}";
        await CreatePublishedJob(jobTitle);

        // Apply
        await Page.GotoAsync($"{BaseUrl}/jobs");
        await Page.Locator(".job-card").Filter(new() { HasText = jobTitle }).Locator(".btn-link").ClickAsync();
        await Page.ClickAsync("a:has-text('Apply Now')");
        string candidateName = $"Applications User {Guid.NewGuid()}";
        await Page.FillAsync("#name", candidateName);
        await Page.FillAsync("#email", "applications@track.com");
        var tempPath = Path.Combine(Path.GetTempPath(), "resume.pdf");
        await File.WriteAllBytesAsync(tempPath, new byte[] { 0x25, 0x50, 0x44, 0x46 });
        await Page.SetInputFilesAsync("#resume", tempPath);
        await Page.ClickAsync("button[type='submit']");

        // Admin login
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", "admin@microats.com");
        await Page.FillAsync("#password", "AdminPassword123!");
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/admin/requisitions"));

        // Navigate to Applications
        await Page.ClickAsync("a:has-text('Applications')");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/admin/applications"));

        // Find candidate in Applied lane
        var appliedLane = Page.Locator("#appliedList");
        var candidateCard = appliedLane.Locator(".card").Filter(new() { HasText = candidateName });
        await Expect(candidateCard).ToBeVisibleAsync();

        // Drag to Interview lane
        var interviewLane = Page.Locator("#interviewList");
        await candidateCard.DragToAsync(interviewLane);

        // Verify it moved
        await Expect(appliedLane.Locator(".card").Filter(new() { HasText = candidateName })).ToHaveCountAsync(0);
        await Expect(interviewLane.Locator(".card").Filter(new() { HasText = candidateName })).ToHaveCountAsync(1);

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
        await Page.FillAsync("#email", "archive@track.com");
        var tempPath = Path.Combine(Path.GetTempPath(), "resume.pdf");
        await File.WriteAllBytesAsync(tempPath, new byte[] { 0x25, 0x50, 0x44, 0x46 });
        await Page.SetInputFilesAsync("#resume", tempPath);
        await Page.ClickAsync("button[type='submit']");

        // Admin login
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", "admin@microats.com");
        await Page.FillAsync("#password", "AdminPassword123!");
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/admin/requisitions"));

        // Navigate to Applications
        await Page.ClickAsync("a:has-text('Applications')");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/admin/applications"));

        var appliedLane = Page.Locator("#appliedList");
        var candidateCard = appliedLane.Locator(".card").Filter(new() { HasText = candidateName });
        await Expect(candidateCard).ToBeVisibleAsync();

        var archiveZone = Page.Locator(".archive-zone");

        // Handle prompt
        void HandleDialog(object? sender, IDialog dialog) => dialog.AcceptAsync("1"); // 1 for Hired
        Page.Dialog += HandleDialog;
        try 
        { 
            await candidateCard.DragToAsync(archiveZone); 
            // Wait a moment for backend
            await Page.WaitForTimeoutAsync(500);
        } 
        finally 
        { 
            Page.Dialog -= HandleDialog; 
        }

        // Verify removed from active pipeline
        await Expect(appliedLane.Locator(".card").Filter(new() { HasText = candidateName })).ToHaveCountAsync(0);

        // Check archived board
        await Page.ClickAsync("a.btn-link:has-text('View Archived Applications')");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/admin/applications/archived"));

        var hiredLane = Page.Locator(".lane").Filter(new() { HasText = "Hired" });
        await Expect(hiredLane.Locator(".card").Filter(new() { HasText = candidateName })).ToHaveCountAsync(1);

        File.Delete(tempPath);
    }
}
