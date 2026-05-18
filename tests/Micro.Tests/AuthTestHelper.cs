using System.Net.Http.Json;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Micro.Tests;

public class AuthTestHelper
{
    private readonly HttpClient _httpClient;
    private readonly string _projectId;
    private readonly string _emulatorHost;

    public AuthTestHelper(string projectId, string emulatorHost)
    {
        _projectId = projectId;
        _emulatorHost = emulatorHost;
        _httpClient = new HttpClient();

        if (FirebaseApp.DefaultInstance == null)
        {
            Environment.SetEnvironmentVariable("FIREBASE_AUTH_EMULATOR_HOST", emulatorHost);
            FirebaseApp.Create(new AppOptions
            {
                ProjectId = projectId,
                Credential = GoogleCredential.FromAccessToken("mock-token")
            });
        }
    }

    public async Task<string> CreateUserAndGetTokenAsync(string email, string password, Dictionary<string, object>? roles = null)
    {
        UserRecord userRecord;
        try
        {
            // 1. Try to create user via Admin SDK
            var args = new UserRecordArgs
            {
                Email = email,
                Password = password,
                EmailVerified = true
            };
            userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(args);
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.EmailAlreadyExists)
        {
            // 2. If user exists, get the existing one
            userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
        }

        // 3. Set custom claims if provided
        if (roles != null)
        {
            await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(userRecord.Uid, roles);
        }

        // 3. Sign in via Emulator REST API to get ID Token
        var signInUrl = $"http://{_emulatorHost}/identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=any-key";
        
        var payload = new
        {
            email,
            password,
            returnSecureToken = true
        };
        
        var response = await _httpClient.PostAsJsonAsync(signInUrl, payload);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get ID token from emulator. Status: {response.StatusCode}, Error: {error}");
        }
        var result = await response.Content.ReadFromJsonAsync<SignInResponse>();
        return result?.IdToken ?? throw new Exception("Failed to get ID token from emulator");
    }

    private record SignInResponse(string IdToken);
}
