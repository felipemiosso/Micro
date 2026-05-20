# Business Specification - Requisition Pages Adjustments

Following the Requisition Overhaul where Requisitions were normalized and multiple independent openings support was added at the schema layer, the user interface and related workflows need to be adjusted. The requisitions management, candidates list/profile, and application status tracking/Kanban board pages must reflect and utilize the new requisition-opening structure.

## User Stories

- As a Recruiter/Hiring Manager, I want to see the progress of all openings for a requisition (e.g., filled/open/cancelled count) directly in the Requisitions List.
- As a Recruiter/Hiring Manager, I want to view detailed status of each individual opening (sequence number, target start date, status, and hired candidate) for finalized requisitions on a detail view.
- As a Recruiter, I want to edit target start dates or cancel unfilled openings of a finalized requisition.
- As a Recruiter, I want to set individual target start dates for openings while creating or editing a draft requisition.
- As a Recruiter, when hiring a candidate (moving status to Archive with resolution Hired), I want to select from the available open openings for the corresponding requisition.
- As a Recruiter, I want to see which requisition and specific opening a candidate was hired into on their profile/details page.

## Decisions

- **Automatic Job and Requisition Closure**: If all openings of a requisition are filled or cancelled, the system will automatically mark the Requisition as Closed and close its associated Job Posting.
- **Hiring Action Restriction**: An application cannot be archived with the resolution `Hired` without associating it with an available `Open` opening.
- **Opening Status Isolation**: Reopening a cancelled opening is not supported for now. Once cancelled, it remains cancelled.
