using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Micro.E2E;

public static class AuthSeedHelper
{
    private static bool _initialized = false;
    public const string AdminEmail = "admin@microats.com";
    public const string AdminPassword = "AdminPassword123!";

    public static async Task SeedAdminUserAsync(string email = AdminEmail)
    {
        if (!_initialized)
        {
            Environment.SetEnvironmentVariable("FIREBASE_AUTH_EMULATOR_HOST", "localhost:9099");
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    ProjectId = "demo-micro-ats",
                    Credential = GoogleCredential.FromAccessToken("mock-token")
                });
            }
            _initialized = true;
        }

        try
        {
            await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
            {
                Email = email,
                Password = AdminPassword,
                EmailVerified = true,
                DisplayName = "Admin User"
            });

            var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
            await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(user.Uid, new Dictionary<string, object>
            {
                { "role", "Admin" }
            });
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.EmailAlreadyExists)
        {
            // User already seeded
        }
    }
}
