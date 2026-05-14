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
