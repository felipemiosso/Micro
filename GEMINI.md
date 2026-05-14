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
    AppDbContext.cs
  Endpoints/      — one file per endpoint group
  Infrastructure/ — cross-cutting concerns (auth, extensions, identity wrappers)
```

Rules:
- No service layer, no repository pattern — use `AppDbContext` directly in endpoints
- Minimal indirection: prefer inline logic over extra classes
- `Infrastructure/` is for framework plumbing (auth handlers, option extensions, identity helpers), not business logic

## Endpoints

- Each endpoint group must live in its own file.
- Never use anonymous functions (lambdas) to define endpoint handlers — always use named methods.