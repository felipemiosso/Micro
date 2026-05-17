# User Profile — Backend Design

## Schema Changes
We need to extend the default `IdentityUser` to include personal information.

### AppUser Entity
Inherits from `IdentityUser`.
- `FullName` (string): User's display name.
- `PhotoUrl` (string, nullable): URL to user's profile picture.

### Data Migrations
- Update `MicroDbContext` to use `AppUser` instead of `IdentityUser`.
- Create migration to add `FullName` and `PhotoUrl` to `AspNetUsers` table.
- Update `DbInitializer` to set `FullName = "System Admin"` for the seed user.

## API Contracts

### GET /api/profile
**Response:** `200 OK`
```json
{
  "id": "guid",
  "email": "admin@microats.com",
  "fullName": "System Admin",
  "photoUrl": null
}
```

## Implementation Notes
- Use `UserManager<AppUser>` in endpoints.
- Map `/api/profile` using `RequireAuthorization()`.
- Extract user ID from `ClaimsPrincipal`.
