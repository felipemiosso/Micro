# Business Specification - Role Management

## Overview
Expand role system beyond hardcoded admin. Enable dynamic creation of roles with specific permissions (actions). Roles are assigned to employee users. Candidates have restricted read-only access to their own profile.

## User Stories

- **As an Admin**, I want to **create new roles** and define which system actions they can perform, so I can delegate tasks securely.
- **As an Admin**, I want to **assign roles to employees** when inviting them to the system, so they have appropriate access levels.
- **As an Employee**, I want to **perform actions** allowed by my role, so I can fulfill my responsibilities.
- **As a Candidate**, I want to **log in** and see my basic profile info, so I can confirm my application data.
- **As an Admin**, I want **system actions** to be automatically discoverable, so I don't have to manually update action lists when new features are added.

## Functional Requirements

- **Dynamic Role Creation**: Admins can create roles with a name and a set of allowed actions.
- **Action Discovery**: System must automatically list available actions (endpoints/methods) for role configuration.
- **Role Assignment**: Admins can assign one or more roles to a user.
- **Access Control**: Users are restricted to actions defined in their assigned roles.
- **Candidate Access**: Restricted to `Name`, `Picture`, and `Email`. No management capabilities.
- **URL Refactoring**: Remove `/admin/` prefix from URLs. Access is determined by role permissions, not URL structure.

## Open Questions (Resolved)
- **Hierarchy**: Flat roles.
- **Overlapping Permissions**: Union of permissions (if multiple roles assigned).
- **Action Representation**: Descriptive names (e.g., "Create Requisition" instead of "Requisition:Post").

## Refactoring
- Rename `AppUser` entity to `User`.
