using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Micro.E2E;

[Trait("Layer", "e2e")]
[Collection("E2E Tests")]
public class UserManagementTests : PageTest
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

    private async Task EnsureRoleExists(string roleName)
    {
        await Page.GotoAsync($"{BaseUrl}/roles/new");
        
        // Wait for page to load
        var nameInput = Page.Locator("input[name='name']");
        await Expect(nameInput).ToBeVisibleAsync();
        
        await nameInput.FillAsync(roleName);
        
        // Check at least one permission
        await Page.Locator("input[type='checkbox']").First.CheckAsync(new() { Force = true });
        
        await Page.ClickAsync("button:has-text('Save Role')");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/roles"));
    }

    [Fact]
    public async Task AC01_AC03_InviteUserAndViewPendingStatus()
    {
        await LoginAsAdmin();
        string roleName = $"Role {Guid.NewGuid().ToString().Substring(0, 8)}";
        await EnsureRoleExists(roleName);
        
        string inviteeEmail = $"newuser-{Guid.NewGuid()}@microats.com";
        string inviteeName = "New Team Member";

        await Page.GotoAsync($"{BaseUrl}/users");
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Team Members");

        await Page.ClickAsync("button:has-text('Invite User')");
        var dialog = Page.Locator("mat-dialog-container").First;
        await Expect(dialog).ToBeVisibleAsync();

        await dialog.Locator("input[formcontrolname='fullName']").TypeAsync(inviteeName);
        await dialog.Locator("input[formcontrolname='email']").TypeAsync(inviteeEmail);
        
        var checkbox = dialog.Locator("label").Filter(new() { HasText = roleName }).Locator("input[type='checkbox']");
        await Expect(checkbox).ToBeVisibleAsync();
        await checkbox.CheckAsync();

        await Page.WaitForTimeoutAsync(500);

        var sendBtn = dialog.Locator("button:has-text('Send Invite')");
        await Expect(sendBtn).ToBeEnabledAsync();
        await sendBtn.ClickAsync();

        await Expect(dialog).Not.ToBeVisibleAsync();

        var row = Page.Locator("table tbody tr").Filter(new() { HasText = inviteeEmail });
        await Expect(row).ToBeVisibleAsync();
        await Expect(row.Locator(".badge-status.badge-amber")).ToHaveTextAsync("Invite Pending");
    }

    [Fact]
    public async Task AC09_UpdateUserRoles()
    {
        await LoginAsAdmin();
        string roleName = $"Role {Guid.NewGuid().ToString().Substring(0, 8)}";
        await EnsureRoleExists(roleName);

        string inviteeEmail = $"roleuser-{Guid.NewGuid()}@microats.com";

        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.ClickAsync("button:has-text('Invite User')");
        var inviteDialog = Page.Locator("mat-dialog-container").First;
        await Expect(inviteDialog).ToBeVisibleAsync();
        
        await inviteDialog.Locator("input[formcontrolname='fullName']").TypeAsync("Role User");
        await inviteDialog.Locator("input[formcontrolname='email']").TypeAsync(inviteeEmail);
        
        var checkbox = inviteDialog.Locator("label").Filter(new() { HasText = roleName }).Locator("input[type='checkbox']");
        await Expect(checkbox).ToBeVisibleAsync();
        await checkbox.CheckAsync();
        
        await Page.WaitForTimeoutAsync(500);

        var sendBtn = inviteDialog.Locator("button:has-text('Send Invite')");
        await Expect(sendBtn).ToBeEnabledAsync();
        await sendBtn.ClickAsync();

        var row = Page.Locator("table tbody tr").Filter(new() { HasText = inviteeEmail });
        await Expect(row).ToBeVisibleAsync();

        await row.Locator("button[mattooltip='Edit Roles']").ClickAsync();

        var editDialog = Page.Locator("mat-dialog-container").First;
        await Expect(editDialog).ToBeVisibleAsync();
        
        var editCheckbox = editDialog.Locator("label").Filter(new() { HasText = roleName }).Locator("input[type='checkbox']");
        await Expect(editCheckbox).ToBeVisibleAsync();
        await editCheckbox.UncheckAsync();

        await editDialog.Locator("button:has-text('Cancel')").ClickAsync();
        await Expect(editDialog).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task AC04_DeletePendingUser()
    {
        await LoginAsAdmin();
        string roleName = $"Role {Guid.NewGuid().ToString().Substring(0, 8)}";
        await EnsureRoleExists(roleName);

        string inviteeEmail = $"deleteuser-{Guid.NewGuid()}@microats.com";

        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.ClickAsync("button:has-text('Invite User')");
        var inviteDialog = Page.Locator("mat-dialog-container").First;
        await Expect(inviteDialog).ToBeVisibleAsync();

        await inviteDialog.Locator("input[formcontrolname='fullName']").TypeAsync("Delete Me");
        await inviteDialog.Locator("input[formcontrolname='email']").TypeAsync(inviteeEmail);
        
        var checkbox = inviteDialog.Locator("label").Filter(new() { HasText = roleName }).Locator("input[type='checkbox']");
        await Expect(checkbox).ToBeVisibleAsync();
        await checkbox.CheckAsync();
        
        await Page.WaitForTimeoutAsync(500);

        var sendBtn = inviteDialog.Locator("button:has-text('Send Invite')");
        await Expect(sendBtn).ToBeEnabledAsync();
        await sendBtn.ClickAsync();

        var row = Page.Locator("table tbody tr").Filter(new() { HasText = inviteeEmail });
        await Expect(row).ToBeVisibleAsync();

        await row.Locator("button[mattooltip='Remove User']").ClickAsync();

        var confirmDialog = Page.Locator("mat-dialog-container").First;
        await Expect(confirmDialog).ToBeVisibleAsync();
        await confirmDialog.Locator("button:has-text('Remove User')").ClickAsync();

        await Expect(row).Not.ToBeVisibleAsync();
    }
}