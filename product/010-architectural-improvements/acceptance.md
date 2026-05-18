# Architectural Improvements — Acceptance Criteria

## Scope
Verification of global security enforcement and structural code integrity.

## Scenarios

### Global Security

#### AC-01: Default Endpoint Protection [layer: integration]
Given a new endpoint is added without explicit auth attributes
When an unauthenticated request hits the endpoint
Then the system returns 401 Unauthorized
And the global fallback policy enforces protection

#### AC-02: Public Endpoint Access [layer: integration]
Given an endpoint marked with `.AllowAnonymous()`
When an unauthenticated request hits the endpoint
Then the system allows access and returns 200 OK

#### AC-03: Documentation Completeness [layer: integration]
Given the project root
When checking for `FALLBACKS.md`
Then the file exists
And it contains a list of all current backend fallbacks

## Out of Scope
- Performance benchmarking of the new routing structure.
- Refactoring of frontend services (contract alignment only).
