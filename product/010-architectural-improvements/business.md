# Architectural Improvements — Business Specification

### Overview
Refactor backend architecture for "secure by default" operations and better code organization. Switch to global authorization policy to prevent accidental data exposure and reorganize endpoint logic into feature-specific folders with dedicated contract files for improved maintainability.

### User Stories
- **Secure by Default**: As a developer, I want all new endpoints to require authentication by default, so I don't accidentally expose sensitive logic.
- **Folder per Feature**: As a developer, I want each endpoint group to have its own folder, so I can find all related logic (routing and DTOs) in one place.
- **Contract Separation**: As a developer, I want DTOs and records moved to dedicated `[Feature]Contracts.cs` files, so the endpoint files stay focused on routing logic.
- **Fallback Audit**: As a developer, I want a documented list of all backend fallbacks, so I can track and eventually remove hardcoded defaults.

### Open Questions
1. Which endpoints are confirmed public? (Default: Health check and Public Job Board).
