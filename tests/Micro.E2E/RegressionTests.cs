using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Micro.E2E;

[Trait("Layer", "e2e")]
[Collection("E2E Tests")]
public class RegressionTests : PageTest
{
    private const string BaseUrl = "http://localhost:4200";

    private async Task Login()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", "admin@microats.com");
        await Page.FillAsync("#password", "AdminPassword123!");
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/admin/requisitions"));
    }

    [Fact]
    public async Task BUG_ProfileNavigation_UsesCorrectId()
    {
        // Login
        await Login();

        // Ensure at least one candidate exists
        await Page.GotoAsync($"{BaseUrl}/admin/candidates");
        var firstRow = Page.Locator("tbody tr").First;
        await Expect(firstRow).ToBeVisibleAsync();

        // Click View Profile
        await firstRow.Locator("a[matTooltip='View Profile']").ClickAsync();

        // Verify URL does NOT contain 'undefined'
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/admin/candidates/[0-9a-fA-F-]"));
        Assert.DoesNotContain("undefined", Page.Url);
        
        // Verify profile loaded (Check for Candidate Profile badge)
        await Expect(Page.Locator("span:has-text('Candidate Profile')")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task BUG_LongEmail_LayoutStability()
    {
        // Login
        await Login();

        // Navigate to candidates
        await Page.GotoAsync($"{BaseUrl}/admin/candidates");
        
        // Find or assume existence of a long email row (can be seeded or just check first)
        var firstRow = Page.Locator("tbody tr").First;
        await firstRow.Locator("a[matTooltip='View Profile']").ClickAsync();

        // Check the email span for truncation styles
        var emailSpan = Page.Locator(".truncate-text");
        await Expect(emailSpan).ToBeVisibleAsync();
        
        // Verify CSS properties for truncation
        var overflow = await emailSpan.EvaluateAsync<string>("el => window.getComputedStyle(el).overflow");
        var textOverflow = await emailSpan.EvaluateAsync<string>("el => window.getComputedStyle(el).textOverflow");
        
        Assert.Equal("hidden", overflow);
        Assert.Equal("ellipsis", textOverflow);
    }

    [Fact]
    public async Task BUG_HiringStage_ScopedToApplication()
    {
        // Login
        await Login();

        // Go to profile
        await Page.GotoAsync($"{BaseUrl}/admin/candidates");
        await Page.Locator("tbody tr").First.Locator("a[matTooltip='View Profile']").ClickAsync();

        // Verify "Applications History" section exists
        await Expect(Page.Locator("h2:has-text('Applications History')")).ToBeVisibleAsync();

        // Verify "Application Stage" exists INSIDE a card, not as global header
        var applicationCard = Page.Locator(".card-honeycomb").First;
        await Expect(applicationCard.Locator(".badge-status")).ToBeVisibleAsync();
        
        // Verify it says "Application Stage" or similar inside card context
        await Expect(applicationCard.Locator("div:has-text('Application Feedback')")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task BUG_Header_HiddenForPublicUsers()
    {
        // Go to home page (unauthenticated)
        await Page.GotoAsync($"{BaseUrl}/jobs");

        // Verify brand logo is present
        await Expect(Page.Locator(".brand")).ToBeVisibleAsync();

        // Verify admin links are NOT present
        await Expect(Page.Locator("a:has-text('Requisitions')")).ToHaveCountAsync(0);
        await Expect(Page.Locator("a:has-text('Applications')")).ToHaveCountAsync(0);
        await Expect(Page.Locator("button[matMenuTriggerFor]")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task BUG_Login_RedirectsIfAuthenticated()
    {
        // Login
        await Login();

        // Attempt to go to /login manually
        await Page.GotoAsync($"{BaseUrl}/login");

        // Verify redirection to dashboard
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/admin/requisitions"));
    }
}
