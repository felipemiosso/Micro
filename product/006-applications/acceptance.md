# Application Applications — Acceptance Criteria

### Kanban Board Interaction
- **AC-01 [layer: e2e]**: Successfully move candidate to a new stage.
  - **Given** I am on the Application Applications page with a candidate in the "Applied" lane
  - **When** I drag the candidate's card to the "Interview" lane
  - **Then** the card should stay in the "Interview" lane
  - **And** a notification should confirm the status update
  - **And** refreshing the page should show the candidate still in the "Interview" lane.

- **AC-02 [layer: e2e]**: Archive candidate from the board.
  - **Given** I am on the Application Applications page with a candidate in the "Offer" lane
  - **When** I drag the candidate's card to the "Archive" drop zone
  - **Then** a modal should appear asking for the archival resolution
  - **When** I select "Hired" and confirm
  - **Then** the candidate's card should be removed from the board
  - **And** the candidate should no longer appear in any lane.

### Filtering
- **AC-03 [layer: e2e]**: Filter board by job posting.
  - **Given** there are applications for "Software Engineer" and "Product Manager"
  - **When** I select "Software Engineer" from the job filter
  - **Then** the board should only show candidate cards for the "Software Engineer" position.

### Archived Board
- **AC-04 [layer: e2e]**: View archived applications by resolution.
  - **Given** there are archived applications with "Hired" and "Rejected" resolutions
  - **When** I navigate to the Archived Applications board
  - **Then** I should see lanes for "Hired", "Rejected", "Declined", and "Withdrawn"
  - **And** the applications should be displayed in their respective resolution lanes.
