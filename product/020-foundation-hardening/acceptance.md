# Spec 020: Foundation Hardening — Acceptance Scenarios

## Refactored Validations and Database Integrity

### AC-01: Remove Duplicate Model Attributes and Verify Migration Consistency
* **Layer:** `[layer: integration]`
* **Given** the `Candidate` domain model does not contain `[Required]` or `[MaxLength]` annotations on its properties,
* **And** the database configuration class `CandidateConfiguration` remains the single source of truth for these constraints,
* **When** a database migration is generated or applied,
* **Then** the resulting database schema structure must remain unchanged,
* **And** existing data must be preserved.

### AC-02: Enforce Explicit Configuration for CostCenter, Department, and SalaryBand
* **Layer:** `[layer: integration]`
* **Given** `CostCenterConfiguration`, `DepartmentConfiguration`, and `SalaryBandConfiguration` classes are created in `Data/Configuration`,
* **And** no entity configurations are registered directly inside `OnModelCreating` in `MicroDbContext.cs`,
* **When** the application builds and tests run,
* **Then** the `MicroDbContext` must successfully apply the configurations from the assembly,
* **And** the database schema must match the explicit requirements defined in these files.

## Frontend Styles and Design System Compliance

### AC-03: Component CSS Migration to Tailwind Utility Classes
* **Layer:** `[layer: e2e]`
* **Given** custom CSS rule declarations in `apply.css`, `application-detail.css`, `success.css`, and `job-posting-edit.css` are deleted,
* **And** equivalent Tailwind CSS utility classes are applied directly in their HTML templates,
* **When** a user navigates to the Job Application page, Application Detail page, Success page, or Job Posting Edit page,
* **Then** the page layout, colors, typography, margins, and padding must remain visually identical to the original design,
* **And** it must strictly adhere to the visual tokens of the Honeycomb Design System.

### AC-04: Remove Unused/Empty CSS Stylesheets
* **Layer:** `[layer: integration]`
* **Given** empty CSS stylesheet files (containing only placeholder comments) are deleted from the features and core directories,
* **And** their references are removed from component decorators (`styleUrl` or `styleUrls`),
* **When** the Angular application compiles,
* **Then** it must build successfully without any missing file references or dependency errors.

## Test Coverage Improvements

### AC-05: Validation Presets Correctness via Unit Tests
* **Layer:** `[layer: integration]`
* **Given** custom unit tests are created for Brazilian format validators (CPF, CNPJ, PIS, CEP), phone numbers, email, and URLs,
* **When** the unit tests run under the test runner (`dotnet test`),
* **Then** they must verify both valid cases (correct checksums, correct formatting) and invalid cases (corrupted checksums, invalid length, invalid formats),
* **And** all tests must pass.

### AC-06: API Endpoint Groups Coverage Verification
* **Layer:** `[layer: integration]`
* **Given** integration tests are written for Candidate, Role, User, UserProfile, and Admin endpoints,
* **When** the integration test suite runs via the test script,
* **Then** all tests must pass, and code coverage metrics must show line coverage at or above 90% for the endpoint classes.

## Query and Transaction Optimizations

### AC-07: Batch Custom Field Loading on Lists
* **Layer:** `[layer: integration]`
* **Given** batch custom field loading helper methods exist,
* **When** listing applications or requisitions,
* **Then** the custom fields for all items in the list must be retrieved using bulk queries instead of executing queries inside a loop,
* **And** the returned custom field values must correctly match the respective items.

### AC-08: Atomic Application Creation and Custom Fields Save
* **Layer:** `[layer: integration]`
* **Given** a job application is submitted with candidate-facing custom fields,
* **When** the transaction/save operation is executed,
* **Then** the application record and all custom field values must be committed to the database in a single database roundtrip,
* **And** if any database validation fails, the entire application creation must roll back atomically.
