# Authentication — Backend Technical Design

## Overview
Migration from ASP.NET Core Identity to Firebase Authentication for improved security, social login capabilities, and reduced server-side state management.

## Authentication Mechanism
- **Provider**: Firebase Authentication.
- **Protocol**: JWT (JSON Web Token).
- **Validation**: Backend validates Firebase ID Tokens using the standard `JwtBearer` middleware.
- **Authority**: `https://securetoken.google.com/<project-id>`
- **Audience**: `<project-id>`

## Database Schema
The `User` table is retained for application-specific profile data but no longer inherits from `IdentityUser`.

### User Entity
| Property | Type | Description |
| :--- | :--- | :--- |
| Id | string (PK) | Firebase UID |
| Email | string | User's email address |
| FullName | string | User's full name |
| PhotoUrl | string? | URL to user's profile picture |

## API Changes
- **AuthEndpoints**: Removed. Registration and Login are handled client-side via Firebase SDK.
- **UserProfileEndpoints**: 
  - `GET /api/profile`: Returns the `User` record for the authenticated user.
  - Automatically creates a local `User` record if the Firebase UID is authenticated but not yet in the database.

## Security
- All admin endpoints require an `Authorization: Bearer <token>` header.
- Token validation includes issuer, audience, and lifetime checks.
- Public endpoints (like job application) remain anonymous.

## Testing
- Integration tests use a `TestAuthHandler` to bypass real Firebase token validation in the `Testing` environment.
- Mock users are seeded with a fixed UID (`test-user-id`) for consistent testing.
