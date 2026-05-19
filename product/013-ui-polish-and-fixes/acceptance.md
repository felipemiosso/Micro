# Acceptance Criteria

## Confirmation Dialogs
- **AC-01 [layer: e2e]**: Closing a requisition must show a custom modal dialog instead of `confirm()`.
- **AC-02 [layer: e2e]**: Finalizing a requisition must show a custom modal dialog instead of `confirm()`.
- **AC-03 [layer: e2e]**: Closing a job posting must show a custom modal dialog instead of `confirm()`.

## Navigation
- **AC-04 [layer: e2e]**: The active route link in the header must be highlighted (honeycomb color) and have an active state indicator.

## Styling Consistency
- **AC-05 [layer: e2e]**: Action buttons in lists (Publish, Close, Finalize, Resume) must use consistent Tailwind classes and consistent padding/typography.

## Candidate Profile
- **AC-06 [layer: e2e]**: The header in the candidate detail page must be correctly aligned and styled.
- **AC-07 [layer: e2e]**: Redundant contact info in the candidate detail sidebar must be removed.
- **AC-08 [layer: e2e]**: "Download Resume" button on the candidate detail page must successfully trigger a file download.

## Role Management
- **AC-09 [layer: integration]**: Clicking "View: List and filter all applications" must only select that specific permission.
- **AC-10 [layer: integration]**: Clicking "View: View application details and feedback" must only select that specific permission.

## User Profile
- **AC-11 [layer: e2e]**: Clicking "Change Password" in the profile page must show a notification "This feature is currently being integrated with our identity provider."
