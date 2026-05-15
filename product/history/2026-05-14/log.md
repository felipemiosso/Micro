# 2026-05-14

**User:** Microsoft.Playwright.PlaywrightException : net::ERR_CONNECTION_REFUSED at http://localhost:4200/login

**Gemini:** The E2E connection error occurred because the backend and frontend servers were not running. I have started the development environment in the background:
- Backend: http://localhost:5249
- Frontend: http://localhost:4200

I verified the fix by running the E2E tests:
- `AuthTests`: 4 passed
- `RequisitionTests`: 5 passed

You can now run the tests successfully.

**User:** di you adjust the script to start the servers when running e2e tests?

**Gemini:** I have now:
1. Created `scripts/test-e2e.sh`: This script automates starting the backend and frontend, waits for them to be healthy, runs the E2E tests, and cleans up the processes afterwards.
2. Updated `scripts/dev.sh`: Fixed the `npm` command to `npm.cmd` to ensure compatibility with Windows environments when running via shell.

**User:** it seems CloseRequisition private method on RequisitionEndpoints is missing somes tests

**Gemini:** I have added the missing integration tests for the `CloseRequisition` endpoint in `tests/Micro.Tests/Endpoints/RequisitionTests.cs`:
- `Close_ExistingRequisition_ChangesStatus`: Verified that closing a valid requisition sets status to `Closed` and updates `ClosedAt`.
- `Close_NonExistentRequisition_ReturnsNotFound`: Verified that attempting to close a non-existent requisition returns `404 Not Found`.

All 6 integration tests for requisitions are now passing.

**User:** now lets move on writing the next feature

**Gemini:** I will start designing **Feature 003: Job Posting Management**. This feature includes:
- Automatic creation of a Job Posting when a Requisition is finalized.
- Public job listings and detail pages.
- Status management (Published/Closed) for Job Postings.
- Independent and dependent closure logic (closing a Requisition closes the Job Posting).

**Gemini:** I have completed the design phase for **Feature 003: Job Posting Management**. The following specification files have been created in `product/003-job-posting-management/`:
- `business.md`: Detailed user stories and overview.
- `acceptance.md`: 6 Acceptance Criteria covering admin and public scenarios.
- `backend.md`: Schema design for the `JobPosting` entity and API contracts.
- `frontend.md`: Design for Admin management components and Public job board components.

The next step is to implement the backend changes, starting with the data model and migrations.

**User:** yes

**Gemini:** Starting backend implementation for **Feature 003: Job Posting Management**.
- Creating `JobPosting` model and `JobPostingStatus` enum.
- Updating `MicroDbContext`.
- Generating and applying database migration.
- Creating API endpoints (`/api/jobs` and `/api/admin/jobs`).
- Connecting Requisition finalization/closure to Job Posting logic.
- Adding integration tests in `JobPostingTests.cs`.
- Building Angular components (`JobBoardComponent`, `JobDetailComponent`, `JobPostingListComponent`, `JobPostingEditComponent`).
- Writing E2E tests in `JobPostingTests.cs`.

All E2E tests for Feature 003 are passing. The feature is complete.

**User:** Create a Configuration folder inside Data and move all models configuration to separated files. Do not code configuration directly in OnModelCreating method. Update agents and GEMINI.md to reflect this instruction as well

**Gemini:** I have completed the requested refactoring:
- **Code:** Created `src/Micro.API/Data/Configuration/JobPostingConfiguration.cs`, moved the configuration from `MicroDbContext`, and updated `OnModelCreating` to use `ApplyConfigurationsFromAssembly()`.
- **Docs:** Updated `GEMINI.md` with the new rule requiring `IEntityTypeConfiguration<T>` in the `Data/Configuration` folder.
- **Agents:** Updated `.agent/agents/tech-lead.md` so the tech lead agent will include the new `Data/Configuration` classes when designing the backend.
- **Verification:** Ran backend integration tests to ensure the database context and configurations are still loaded properly. All tests passed.

**User:** running coverage.sh is making 12 tests to fail. maybe the server is not up?

**Gemini:** You were right. The `coverage.sh` script was previously running `dotnet test` against the entire solution, which included the `Micro.E2E` tests. Since the E2E tests require the backend and frontend servers to be running, they were failing. 

I've updated `scripts/coverage.sh` to explicitly target `tests/Micro.Tests/Micro.Tests.csproj` since E2E tests do not generate .NET code coverage anyway. Now, `sh scripts/coverage.sh` runs only the backend unit/integration tests and generates the report successfully.

**User:** we have one broken e2e test

**Gemini:** I analyzed the test output and found that `AC02_UpdateDraftRequisition` was failing intermittently due to a race condition. Playwright was navigating to the edit page and typing the new title instantly, but a fraction of a second later, the API response for the requisition details would arrive and overwrite Playwright's input back to the original title via `patchValue`.

I fixed this by adding `await Expect(Page.Locator("#title")).ToHaveValueAsync(title);` right before the typing steps in both `RequisitionTests.cs` and `JobPostingTests.cs`. This ensures Playwright waits for the form to be fully populated by the API before it starts interacting with it. All 12 E2E tests now pass reliably.
