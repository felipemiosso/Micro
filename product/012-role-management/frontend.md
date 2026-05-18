# Frontend Design - Role Management

## Components

### Role Management
- `RoleListComponent`: Table of existing roles.
- `RoleEditorComponent`: Form to create/edit roles.
  - Searchable list of checkboxes for "Actions".
  - Actions grouped by "Resource" (e.g., Application, Job, Requisition).

### User Management
- Update `InviteUserComponent` to fetch roles and show a selection dropdown/multi-select.

## Services
- `RoleService`:
  - `getRoles()`
  - `createRole(role)`
  - `getAvailableActions()`

## State Management (Signals)
- `currentUserRoles`: Signal in `AuthService` populated on login.
- `hasPermission(resource, action)`: Computed/Helper to check if current user can perform an action.

## Routing & Guards
- Refactor routes to remove `/admin` segments.
- Update `PermissionGuard` to check against dynamic permissions instead of hardcoded `isAdmin`.

## UI/UX
- Hide/Disable buttons and menu items based on `hasPermission` signal.
- Candidates see a minimal "My Profile" view, others see the management dashboard (filtered by permission).
