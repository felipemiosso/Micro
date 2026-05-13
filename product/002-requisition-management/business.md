# Business Overview - Requisition Management

Allows Admin users to manage internal hiring needs through Requisitions. This feature is the first step in the hiring lifecycle.

## User Stories
- **Create Requisition:** As an Admin, I want to create a Requisition with a title, department, and number of openings so I can start the hiring process.
- **List Requisitions:** As an Admin, I want to see a list of all Requisitions and their status.
- **Update Requisition:** As an Admin, I want to edit a Requisition while it is in "Draft" status.
- **Finalize Requisition:** As an Admin, I want to "Finalize" a Requisition so it becomes ready for public posting.
- **Close Requisition:** As an Admin, I want to "Close" a Requisition to stop the hiring process for that role.

## Business Rules
- Statuses: `Draft`, `Finalized`, `Closed`.
- Only `Draft` requisitions can be edited.
- Once `Finalized`, a requisition becomes read-only (except for closing).
- Closing a requisition is a terminal action (cannot be re-opened).

## Open Questions
- Should we track who created the requisition? (Assumption: For MVP, we only have one 'Admin', but we'll store the created date).
