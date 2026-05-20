**User:** now that we revemaped requisition, we need to adjust the pages. requisitions, candidates and applications must be adjusted to the new requisition/position format. create a new spec with the adjusments

**Gemini:** I have created the new feature specification for the Requisition Pages Adjustments in the directory `product/016-requisition-pages-adjustments/`.

The specification contains:
1. [business.md](file:///C:/Users/felip/source/Micro/product/016-requisition-pages-adjustments/business.md): Explains the user stories, goals, and decisions (e.g. automatic requisition/job closure when all openings are filled or cancelled).
2. [acceptance.md](file:///C:/Users/felip/source/Micro/product/016-requisition-pages-adjustments/acceptance.md): Sets out the observable business scenarios and behavior criteria (e.g. tracking multiple openings, individual target dates, and hiring candidate validation).
3. [backend.md](file:///C:/Users/felip/source/Micro/product/016-requisition-pages-adjustments/backend.md): Details the API contracts and DB logic adjustments (e.g. updating the status update endpoint to support setting a specific opening ID).
4. [frontend.md](file:///C:/Users/felip/source/Micro/product/016-requisition-pages-adjustments/frontend.md): Outlines the UI/component design changes in Angular (e.g. the Archive Dialog dropdown for selection of openings).

Please review the specifications and let me know if they look good or if you would like any modifications. Once approved, I can prepare the implementation plan and start coding!

**User:** do it

**Gemini:** I have created the implementation plan for the adjustments. Please review [implementation_plan.md](file:///C:\Users\felip\.gemini\antigravity-cli\brain\42335703-54d0-44f7-bd2b-f5c8abc7e5f0\implementation_plan.md) and provide your feedback or approval so we can begin coding.

**User:** do it

**User:** /caveman full

**Gemini:** Backend test running in background. Waiting for results. Verification of frontend build next.

**User:** ok

**User:** always use the scripts to run tests, since we have emulators

**User:** there is no need to test e2e since they are going to broke due layout changes. lets keep integration working for now

**Gemini:** Frontend build success. Backend integration tests passed (21/21). Changes verified.

**User:** requisitons have this unknown status, why is that? also, im able to see anywhere the new fields we added on req. im also not able to see the independent positions.

**Gemini:** Unknown status because backend returns string enums but frontend declared numeric enums. Changed frontend enums to strings. Clickable requisition titles added to list view, routing users to full detail/tracking view with new fields and positions table. Action button visibility added for direct navigation. Tests fully green.

**User:** there is update requisition button in this page "requisitions/edit" but when I click it i see an error "Failed to save requisition."

**Gemini:** Empty date strings on the frontend caused deserialization/binding failure (400 Bad Request) on C# backend DateTime fields. Added sanitization logic to map empty target start date strings to null.

**User:** create a new integration test for this scenario

**Gemini:** Created integration test `Update_DraftRequisition_WithNullTargetStartDate_SavesNull` in `RequisitionTests.cs` to ensure that null/cleared target start dates on the requisition and its openings are correctly persisted to the database. All 22/22 integration tests passed.

**User:** it worked, but now im getting "Only draft requisitions can be updated.". if that is the case, i think the button should be disable, right?

**Gemini:** Removed dependency on form.enabled state for submit button rendering. Explicitly disabled the "Update Requisition" submit button when in edit mode and the requisition status is not Draft. All tests green.

**User:** the app is working fine but Im gettring when I refresh the page. 
http://localhost:9099/identitytoolkit.googleapis.com/v1/accounts:lookup?key=AIzaSyAsUYRSb6bz6MdMARNVS7YnEniiYzFGfT8 net::ERR_CONNECTION_REFUSED

**Gemini:** Firebase Auth Emulator is not running on port 9099. Start the full development environment using the script: `bash ./scripts/dev.sh`.

**User:** when I enter requisiton edit page:
requisition-form.ts:84 ERROR RuntimeError: NG0100: ExpressionChangedAfterItHasBeenCheckedError: Expression has changed after it was checked. Previous value: '82'. Current value: '-1'. Expression location: _RequisitionFormComponent component. Find more at https://v21.angular.dev/errors/NG0100
    at RequisitionFormComponent_Template (requisition-form.html:102:7)

**Gemini:** Replaced `form.enabled` check in the template's conditional block (`@if`) with `requisition()?.status === Status.Draft`. This prevents `ExpressionChangedAfterItHasBeenCheckedError` during page load/initialization when the form is programmatically disabled in the subscription callback.

**User:** candidate page application history there is a bug: Current Stage Unknown

**Gemini:** `CandidateEndpoints.cs` was casting `a.Status` and `a.ArchivalResolution` to `(int)`, sending numeric values (0, 1, 2...) to the frontend. Frontend enums are strings (`'Applied'`, `'Interview'`...) so every value hit `default: return 'Unknown'`. Fixed by changing `CandidateContracts.cs` fields to `string` and replacing `(int)` casts with `.ToString()`. All 22/22 integration tests passed.

**User:** /caveman full

**Gemini:** Full audit pass. Fixed two additional bugs found during review: (1) `application-list.html` status badge was hardcoded to `badge-slate` regardless of status — replaced with dynamic `badge-amber/blue/green/zinc` bindings. (2) `applications-board.html` used legacy `*ngFor` directive — replaced with modern `@for` control flow. Build clean, all tests green.
