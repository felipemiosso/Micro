using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Micro.E2E;

[Trait("Layer", "e2e")]
[Collection("E2E Tests")]
public class AuthTests : PageTest
{
    private const string BaseUrl = "http://localhost:4200";

    [Fact]
    public async Task AC01_SuccessfulAdminLogin()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/login");

        // Act
        await Page.FillAsync("#email", "admin@microats.com");
        await Page.FillAsync("#password", "AdminPassword123!");
        await Page.ClickAsync("button[type='submit']");

        // Assert
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/admin/requisitions"));
    }

    [Fact]
    public async Task AC02_FailedLogin_InvalidCredentials()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/login");

        // Act
        await Page.FillAsync("#email", "admin@microats.com");
        await Page.FillAsync("#password", "WrongPassword");
        
        var submitButton = Page.Locator("button[type='submit']");
        await Expect(submitButton).ToBeEnabledAsync();
        
        var responseTask = Page.WaitForResponseAsync("**/api/auth/login");
        await submitButton.ClickAsync();
        await responseTask;

        // Assert
        var error = Page.Locator(".error-summary");
        await Expect(error).ToBeVisibleAsync();
        await Expect(error).ToHaveTextAsync("Invalid email or password");
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
    }

    [Fact]
    public async Task AC03_UnauthorizedAccessProtection()
    {
        // Act
        await Page.GotoAsync($"{BaseUrl}/admin/requisitions");

        // Assert
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/login"));
    }

    [Fact]
    public async Task AC04_AdminLogout()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#email", "admin@microats.com");
        await Page.FillAsync("#password", "AdminPassword123!");
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/dashboard|/admin/requisitions"));

        // Act
        // Assuming there's a logout button in the app shell/header
        // Looking at src/Micro.Web/src/app/app.ts to find logout logic/selector
        await Page.ClickAsync("button:has-text('Logout')");

        // Assert
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
    }
}
