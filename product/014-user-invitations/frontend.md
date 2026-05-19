# Frontend Design - User Invitations

## Components

### Users Dashboard (`src/app/features/users/list`)
- **Users Table**: Shows Full Name, Email, Roles, Status (Active vs Invite Pending).
- **Actions Menu**:
  - `Edit Roles` (Always available)
  - `Resend Invite` (Only if Invite Pending)
  - `Delete User` (Destructive, uses `ConfirmDialogComponent`)

### Invite User Dialog (`src/app/features/users/invite`)
- Triggered by "Invite User" button on the dashboard.
- Form: Full Name, Email, Multi-select checkbox list for Roles.
- Uses standard honeycomb inputs and `btn-primary`.

### Edit User Roles Dialog (`src/app/features/users/edit-roles`)
- Manages roles for an active user.
- Pre-selects current roles.

## Routing
- `/users` -> User Management Dashboard

## State Management
- `UserService` with Signal for `users()`.
- API interactions for creating users (inviting), resending invites, deleting users, and updating roles.
