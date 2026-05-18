# Backend Design - Role Management

## Schema Changes

### Role Entity
- `Id` (Guid)
- `Name` (string)
- `Permissions` (List<string>) - Stores action identifiers (e.g., `JobPosting:View`, `Requisition:Create`)

### User Entity (Renamed from AppUser)
- `Roles` (Collection<Role>) - Many-to-many relationship

## Action Discovery Mechanism
- Define a custom attribute `[ResourceAction(Resource, Action, Description)]` for endpoints.
- `Description` will be used for display in the UI.

## Access Control Logic
- Custom Authorization Policy/Requirement.
- `AuthUser` record will be updated to include `Roles` and flattened `Permissions` (Union of all roles).
- Middleware or Filter to match requested endpoint's `ResourceAction` against user's `Permissions`.

## API Contracts

### Role Management
- `GET /api/roles` - List all roles
- `POST /api/roles` - Create role `{ name: string, actions: string[] }`
- `GET /api/roles/available-actions` - List all discoverable actions

### User Management
- `POST /api/users/invite` - Update to include `roleIds`

## URL Refactoring
- Move all `/api/admin/*` to `/api/*`.
- Apply appropriate `[ResourceAction]` to each.
