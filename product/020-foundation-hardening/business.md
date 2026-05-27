# Spec 020: Foundation Hardening

## Overview
Assess the current system's technical health, standards compliance, and test coverage. Harden the application's foundation by addressing technical debt across the codebase. On the backend, we will remove duplicate constraint configurations (where validations are defined on both the model class attributes and EF Core Fluent configurations) and ensure that every database entity has an explicit configuration file. On the frontend, we will align styles with the Honeycomb Design System by migrating component-specific CSS styling to Tailwind utility classes, deleting unused styling files, and removing redundant stylesheet decorators. Finally, we will significantly increase test coverage (aiming for 90%+ line coverage and improved branch coverage) by writing unit tests for custom validation algorithms and adding missing integration tests for currently untested API endpoint groups.

## User Stories

* **Refactored Model Validations:** As a Developer, I want database validation constraints configured in exactly one place (the EF Core configuration files) so that the codebase is easier to maintain and lacks redundant validation declarations.
* **Explicit Entity Configuration:** As a Developer, I want every Entity Framework model to have a dedicated class implementing `IEntityTypeConfiguration<T>` in the `Data/Configuration` directory so that all schema definitions comply with our architectural rules and avoid implicit model binding.
* **Tailwind Alignment & Style Cleanup:** As a Developer, I want component-specific custom CSS styles migrated to inline Tailwind classes and all unused CSS files removed from the frontend project, ensuring that our visual layer is clean, centralized, and strictly adheres to the Honeycomb Design System.
* **Custom Field Validator Verification:** As a QA Engineer, I want the custom validation presets (CPF, CNPJ, PIS, CEP, CNH, Phone, URL, Email) to be thoroughly unit tested so that I can guarantee the correctness of complex validation and formatting logic.
* **API Endpoints Coverage:** As a QA Engineer, I want integration tests for candidate, role, user, user profile, and admin endpoints so that core business routes are protected from regressions.

## Open Questions

* **E2E Test Coverage:** Should we write Playwright E2E tests for the currently uncovered routes (Candidate, User, Role, Admin) as part of this hardening sprint, or should we prioritize backend integration tests and utility unit tests? *(Recommendation: Focus on integration tests first to cover core business rules, then expand E2E tests in a subsequent iteration).*
