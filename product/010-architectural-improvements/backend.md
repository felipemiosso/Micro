# Architectural Improvements — Backend Design

## Overview
Refactor `Program.cs` and `Endpoints` folder structure. Enforce global authorization and improve code separation.

## API Endpoints

### Authorization Policy
- **Global Policy**: Set `FallbackPolicy` in `Program.cs` to require `AuthenticatedUser`.
- **Public Exceptions**: Use `.AllowAnonymous()` for:
  - `HealthCheckEndpoints`
  - `JobPostingEndpoints` (Public routes only)
  - `ApplicationEndpoints` (Public apply route only)

### Folder Structure
Each endpoint group moves from `Endpoints/*.cs` to `Endpoints/[Name]/`:
- `Endpoints/[Name]/[Name]Endpoints.cs`: Mapping and logic.
- `Endpoints/[Name]/[Name]Contracts.cs`: Records and DTOs.

**Affected Groups**:
- `Application`
- `Auth` (Already removed, verify clean)
- `HealthCheck`
- `JobPosting`
- `Requisition`
- `UserProfile`

## Documentation
- Create `FALLBACKS.md` in project root.
- Enumerate logic where "magic" defaults are used (e.g., User creation fallback name).

## Technical Risks & Dependencies
- **Breaking Changes**: Moving files and changing namespaces requires updating all references and imports.
- **Auth Lockout**: Ensure global policy doesn't block Swagger/Scalar in development.
