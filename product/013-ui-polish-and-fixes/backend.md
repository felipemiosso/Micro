# Technical Design - UI Polish

## Permission Bug Fix
The `ApplicationEndpoints.cs` has duplicate permission keys for two different actions.
- Action: `GetAdminApplications` -> `[ResourceAction("Application", "View", ...)]`
- Action: `GetApplicationDetail` -> `[ResourceAction("Application", "View", ...)]`

**Change**: Update `GetApplicationDetail` to use a unique action name.
- Action: `GetApplicationDetail` -> `[ResourceAction("Application", "Details", "View application details and feedback")]`

## Resume Download
Ensure the backend returns correct headers for PDF download.
The frontend `ApplicationDetailComponent` needs to append the anchor tag to the document body before clicking to ensure compatibility.

## API Changes
No schema changes required.
Verify `ActionDiscovery` correctly identifies the new `Details` action for the `Application` resource.
