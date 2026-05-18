# Authentication System Overview

This document describes how authentication is implemented in the Micro project for both real users and testing.

## Real-World Authentication (Production/Development)

The system uses **Firebase Authentication** with JWT tokens.

### 1. Configuration
Authentication is configured in `src/Micro.API/Infrastructure/Auth/AuthExtensions.cs`. It sets up the `JwtBearer` scheme to validate tokens issued by Firebase.

- **Authority:** `https://securetoken.google.com/{FirebaseProjectId}`
- **Audience:** `{FirebaseProjectId}`

### 2. Implementation Logic
- **`AuthUser` Record:** Located in `src/Micro.API/Infrastructure/Auth/AuthUser.cs`. It implements a `BindAsync` method, which is a .NET Minimal APIs feature for [custom binding](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#custom-binding).
- **Identity Extraction:** When an endpoint uses `AuthUser user` as a parameter, the `BindAsync` method automatically:
    1. Checks if the request is authenticated.
    2. Extracts claims like `NameIdentifier` (UID), `Email`, `Name`, and `picture` (PhotoUrl).
    3. Returns an `AuthUser` instance or `null` if unauthenticated.

### 3. Global Authorization
- In `Program.cs`, the authorization middleware is configured with a **Fallback Policy** that requires authentication for all endpoints by default, unless explicitly marked with `.AllowAnonymous()`.

---

## Authentication in Testing

Testing uses a specialized **Test Authentication Scheme** to bypass Firebase and simulate users.

### 1. Test Scheme Setup
In `tests/Micro.Tests/TestDatabaseFixture.cs`, the `WebApplicationFactory` is configured to override the default authentication during tests:

```csharp
builder.ConfigureTestServices(services =>
{
    services.AddAuthentication(defaultScheme: "Test")
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
            "Test", options => { });
});
```

### 2. `TestAuthHandler`
Located in `tests/Micro.Tests/TestAuthHandler.cs`, this handler:
- Intercepts requests.
- Checks for an `Authorization` header.
- If present (regardless of the token content), it creates a `ClaimsPrincipal` with a hardcoded test user identity:
    - **Email/Name:** `test@microats.com`
    - **ID:** `test-user-id`

### 3. Usage in Tests
Tests use the `HttpClient` from the fixture. To simulate an authenticated request, they must include an `Authorization` header:

```csharp
Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
```

---

## Current Limitations & Potential Improvements

1. **Hardcoded Test User:** `TestAuthHandler` currently returns the same user for any request with an `Authorization` header. This makes testing scenarios with different users or roles difficult.
2. **Missing Claims:** Some domain logic might require additional claims (e.g., Roles) that are not currently simulated in the test handler.
3. **Manual Header Assignment:** Tests have to manually set the header on the `HttpClient`.
