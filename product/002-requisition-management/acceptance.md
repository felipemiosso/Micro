# Acceptance Criteria - Requisition Management

## Happy Path: Lifecycle
- **Given** I am an Admin
- **When** I create a Requisition with "Software Engineer", "Engineering", and 2 openings
- **Then** a new Requisition is saved with status "Draft"
- **And** I can see it in the Requisition list.

- **Given** a "Draft" Requisition exists
- **When** I update the title to "Senior Software Engineer"
- **Then** the change is saved successfully.

- **Given** a "Draft" Requisition exists
- **When** I click "Finalize"
- **Then** the status changes to "Finalized"
- **And** I can no longer edit the details.

- **Given** a "Finalized" Requisition exists
- **When** I click "Close"
- **Then** the status changes to "Closed"
- **And** it is marked as inactive in the list.

## Business Rules & Constraints
- **Admin Only:** Only users with the "Admin" role can access Requisition management pages or APIs.
- **Draft Only Edits:** If I try to access the Edit page for a "Finalized" requisition, I should be redirected or see a read-only view.
- **Validation:** Creating/Updating requires Title (not empty), Department (not empty), and Openings (> 0).

## User-Visible Errors
- "Cannot edit a finalized requisition."
- "Title is required."
- "Number of openings must be at least 1."
