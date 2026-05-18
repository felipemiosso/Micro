# Acceptance Criteria - Role Management

## Roles & Permissions

### AC-01: Admin creates a new role [layer: e2e]
**Given** an authenticated Admin user
**When** they create a role named "HR Manager" with actions "View Requisitions", "Create Requisitions"
**Then** the role is saved in the system
**And** it becomes available for assignment to users.

### AC-02: Admin assigns role to employee [layer: integration]
**Given** an Admin is inviting a new employee user
**When** they select the "HR Manager" role during the invitation
**Then** the user is created with the assigned role.

### AC-03: Employee access based on role union [layer: e2e]
**Given** an Employee with two roles: "HR Manager" (Requisitions) and "Job Editor" (Jobs)
**When** they attempt to "View Requisitions"
**Then** action is allowed.
**When** they attempt to "Update Job Posting"
**Then** action is allowed.
**When** they attempt to "Delete User" (not in either role)
**Then** action is denied (403 Forbidden).

### AC-04: Candidate restricted access [layer: e2e]
**Given** an authenticated Candidate user
**When** they view their profile
**Then** they see their Name, Email, and Picture
**But** they cannot access any `/api/requisitions` or `/api/jobs` endpoints.

### AC-05: Dynamic action list [layer: integration]
**Given** a new endpoint is added to the backend code
**When** an Admin views the role creation screen
**Then** the new action automatically appears in the list of selectable permissions.

## URL Refactoring

### AC-06: Unified URL structure [layer: e2e]
**Given** the current system uses `/api/admin/jobs`
**When** this feature is implemented
**Then** the URL becomes `/api/jobs` for all users (permissions handled by role).
