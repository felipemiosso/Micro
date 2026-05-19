using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Micro.E2E;

[Trait("Layer", "e2e")]
[Collection("E2E Tests")]
public class JobPostingTests : PageTest
{
    private const string BaseUrl = "http://localhost:4200";

    private async Task LoginAsAdmin()
    {
        string email = $"admin-{Guid.NewGuid()}@microats.com";
        await AuthSeedHelper.SeedAdminUserAsync(email);
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", email);
        await Page.FillAsync("#password", AuthSeedHelper.AdminPassword);
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/requisitions"));
    }

    [Fact]
    public async Task AC01_AutomaticCreationOnFinalization()
    {
        await LoginAsAdmin();
        string title = $"Job {Guid.NewGuid()}";

        // Create Requisition
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Engineering");
        await Page.FillAsync("#openings", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");

        // Finalize Requisition
        var reqRow = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await Expect(reqRow).ToBeVisibleAsync();
        
        await reqRow.Locator(".action-btn.finalize").ClickAsync();
        await Page.ClickAsync("button:has-text('Finalize')");
        
        // Wait for finalization to complete before navigating
        await Expect(reqRow.Locator(".badge-status")).ToHaveTextAsync("Finalized");

        // Check Job Posting
        await Page.GotoAsync($"{BaseUrl}/job-management");
        var jobRow = Page.Locator(".job-table tbody tr").Filter(new() { HasText = title });
        await Expect(jobRow).ToBeVisibleAsync();
        await Expect(jobRow.Locator(".badge-status")).ToHaveTextAsync("Published");

        // Check Public Board
        await Page.GotoAsync($"{BaseUrl}/jobs");
        var jobCard = Page.Locator(".job-card").Filter(new() { HasText = title });
        await Expect(jobCard).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AC02_EditJobPostingDetails()
    {
        await LoginAsAdmin();
        string title = $"Editable Job {Guid.NewGuid()}";
        string newDesc = "This is a new public description.";

        // Pre-requisite: Finalized Requisition
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Product");
        await Page.FillAsync("#openings", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");
        
        var reqRow = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await Expect(reqRow).ToBeVisibleAsync();
        
        await reqRow.Locator(".action-btn.finalize").ClickAsync();
        await Page.ClickAsync("button:has-text('Finalize')");
        
        // Wait for finalization to complete before navigating
        await Expect(reqRow.Locator(".badge-status")).ToHaveTextAsync("Finalized");

        // Edit Job Posting
        await Page.GotoAsync($"{BaseUrl}/job-management");
        var jobRow = Page.Locator(".job-table tbody tr").Filter(new() { HasText = title });
        await Expect(jobRow).ToBeVisibleAsync(); // Ensure row is loaded
        await jobRow.Locator("a[mattooltip='Edit Job']").ClickAsync();

        // Wait for the API to populate the form
        await Expect(Page.Locator("#title")).ToHaveValueAsync(title);

        await Page.FillAsync("#description", newDesc);
        await Page.FillAsync("#requirements", "Must have 5 years of experience.");
        await Page.ClickAsync("button:has-text('Save Changes')");

        // Wait for redirect back to admin list
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/job-management");

        // Verify Publicly
        await Page.GotoAsync($"{BaseUrl}/jobs");
        await Page.ClickAsync($".job-card:has-text('{title}') .btn-link");
        // Ensure we are on the detail page
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/jobs/"));
        
        // Use a more specific selector to avoid strict mode violation
        var descriptionSection = Page.Locator("section").Filter(new() { HasText = "About the Role" });
        await Expect(descriptionSection.Locator(".formatted-text")).ToContainTextAsync(newDesc);
    }

    [Fact]
    public async Task AC03_ManualJobPostingClosure()
    {
        await LoginAsAdmin();
        string title = $"Closable Job {Guid.NewGuid()}";

        // Pre-requisite: Published Job
        await Page.GotoAsync($"{BaseUrl}/requisitions/new");
        await Page.FillAsync("#title", title);
        await Page.FillAsync("#department", "Sales");
        await Page.FillAsync("#openings", "1");
        await Page.ClickAsync("button:has-text('Create Requisition')");
        var reqRow = Page.Locator(".requisition-table tbody tr").Filter(new() { HasText = title });
        await Expect(reqRow).ToBeVisibleAsync();

        await reqRow.Locator(".action-btn.finalize").ClickAsync();
        await Page.ClickAsync("button:has-text('Finalize')");
        
        // Wait for finalization to complete before navigating
        await Expect(reqRow.Locator(".badge-status")).ToHaveTextAsync("Finalized");

        // Close Job Posting
        await Page.GotoAsync($"{BaseUrl}/job-management");
        var jobRow = Page.Locator(".job-table tbody tr").Filter(new() { HasText = title });
        await Expect(jobRow).ToBeVisibleAsync(); // Wait for the job to appear in the list

        await jobRow.Locator("button[mattooltip='Close Job']").ClickAsync();
        await Page.ClickAsync("button:has-text('Close Job')");

        // Verify Closed status
        await Expect(jobRow.Locator(".badge-status")).ToHaveTextAsync("Closed");

        // Verify Hidden on Public Board
        await Page.GotoAsync($"{BaseUrl}/jobs");
        var jobCard = Page.Locator(".job-card").Filter(new() { HasText = title });
        await Expect(jobCard).Not.ToBeVisibleAsync();
    }

}
