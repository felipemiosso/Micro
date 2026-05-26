# Micro

## Stack

- **Backend:** .NET 10, Minimal APIs
- **Frontend:** Angular (served separately, not by the API)
- **Database:** PostgreSQL with EF Core and EF Migrations
- **Testing:** xUnit with Respawn, Playwright for E2E
- **Logging:** Serilog with file sink
- **API Docs:** Swagger (development mode only)
- **Infrastructure:** No Docker

## Logging

- All conversations between the user and the agents must be logged verbatim, without any summaries, cuts, or omissions. Every turn must be recorded exactly as it occurred.
- Logs live in `product/history/YYYY-MM-DD/log.md`.
- Log format must use `**User:**` and `**Gemini:**` for each turn.

## Specs

Feature specs live in `product/NNN-spec-name/` folders (e.g. `product/001-authentication/`). Each folder contains up to four files:

- `business.md` — Overview, User Stories, Open Questions (written by product-owner agent; no Stakeholders, Out of Scope, or Acceptance Criteria sections)
- `acceptance.md` — observable business behavior only: happy path, auth/permissions, user-visible errors, business rules (written by qa-engineer agent; no cookie/header internals, no concurrency, no schema validation)
  - Every scenario MUST have a unique ID per file (AC-01, AC-02, etc.).
  - Every scenario MUST be labeled with its target layer: `[layer: e2e]` or `[layer: integration]`.
- `backend.md` — technical design: schema, EF entities, API contracts, LLM integration (written by tech-lead agent)
- `frontend.md` — Angular component design, service contracts, routing, state (written by frontend-lead agent)


## Testing

- All tests must follow the Arrange / Act / Assert (AAA) pattern with `// Arrange`, `// Act`, `// Assert` comments.
- Test folder structure must follow the backend project structure (e.g., `tests/Micro.Tests/Endpoints/` matches `src/Micro.API/Endpoints/`).
- E2E tests using Playwright live in `tests/Micro.E2E/`.
- Code coverage is mandatory. Use `coverlet.collector` for data collection.
- Use `reportgenerator` to generate human-readable coverage reports in HTML format.

## Project Structure

```
src/Micro.API/
  Data/
    Models/       — domain models (EF entities and config-bound models)
    Migrations/   — EF Core migrations
    Configuration/— EF Core entity type configurations
    MicroDbContext.cs
  Endpoints/      — one file per endpoint group
  Infrastructure/ — cross-cutting concerns (auth, extensions, identity wrappers)
    Auth/         — AuthUser.cs (BindAsync identity), AuthExtensions.cs
    Database/     — DatabaseExtensions.cs (migrations, seeding)
    Logging/      — LogExtensions.cs (Serilog)
    OpenApi/      — SwaggerExtensions.cs (OpenAPI/Scalar)
```

Rules:
- No service layer, no repository pattern — use `MicroDbContext` directly in endpoints
- Minimal indirection: prefer inline logic over extra classes
- `Infrastructure/` is for framework plumbing:
    - **Identity:** Use the `AuthUser` record with `BindAsync` for all authenticated endpoints to extract user identity from claims.
    - **Extensions:** Keep configuration logic in specialized extension classes within `Infrastructure/` subfolders.
- All Entity Framework model configurations must be in separated files implementing `IEntityTypeConfiguration<T>` inside the `Data/Configuration` folder. Do not configure models directly in `OnModelCreating`.

## Frontend Rules

- **Design System:** All UI development MUST follow the standards defined in `product/design-system.md`. Use the provided Tailwind utility classes for consistency.
- **Custom UI Components:** 
  - NEVER use native `confirm()` or `alert()`. Use `ConfirmDialogComponent` from `src/app/core/ui/confirm-dialog.ts`.
  - Use `NotificationService` for all user feedback (success/error/info).
- **Badge Standards:** Use `.badge-status` with its color variants (`.badge-amber`, `.badge-green`, etc.) for all status-like indicators.
- **Deep Linking:** Implement fragment-based scrolling and use the `.highlight-pulse` animation when navigating to specific items (e.g., Kanban cards).
- **Angular Signals:** Use Angular Signals (`signal`, `computed`, `effect`) for all component state and data flow. Avoid manual change detection (`ChangeDetectorRef.detectChanges()`) and raw property assignments for reactive data.
- **Standalone Components:** All new components must be standalone.
- **Style Rules:** Do not use inline CSS styles (`style="..."` attributes) unless extremely necessary; prefer Tailwind utility classes.