# Acceptance Criteria

## Invitation Management
- **AC-01 [layer: e2e]**: Admin can generate an invitation for a specific email address and select one or more Roles.
- **AC-02 [layer: integration]**: The system must prevent inviting an email address that is already registered in the system.
- **AC-03 [layer: e2e]**: Admin can view a list of all users, with visual indicators for users who have "Invite Pending".
- **AC-04 [layer: e2e]**: Admin can delete a pending user, removing their access and record from the system.
- **AC-05 [layer: integration]**: Admin can trigger a "Resend Invite" action for pending users.

## User Onboarding
- **AC-06 [layer: manual]**: User receives standard Firebase password reset email.
- **AC-07 [layer: manual]**: Upon setting password and logging in for the first time, the `IsInvitePending` flag is automatically cleared (implicitly via successful login).

## User Management
- **AC-08 [layer: e2e]**: Admin can view a list of all active users in the system.
- **AC-09 [layer: e2e]**: Admin can update the roles assigned to an existing active user.
