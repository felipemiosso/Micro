# Technical Design - Frontend

## Components

### Requisitions Pages
- **Requisitions List (`RequisitionListComponent`)**:
  - Update the "Openings" column to show `hiredCount / totalCount` (e.g. `1 / 3`). Count how many openings have `status === OpeningStatus.Filled`.
- **Requisitions Detail / Form Page (`RequisitionFormComponent`)**:
  - **Draft State**:
    - Add a form array/dynamic group for individual target start dates if `openingsCount > 1`.
    - Provide a default date input that propagates to all openings unless overridden.
  - **Finalized / Closed State**:
    - Display an "Openings" table section:
      - Columns: `#` (Sequence Number), `Target Start Date`, `Status`, `Hired Candidate` (link to `/candidates/:id` if filled).
      - Inline controls or a dialog to:
        - Edit `Target Start Date` (updates date via `updateOpening` API).
        - Cancel Opening (button showing confirm dialog, calls `updateOpening` with `Status = Cancelled`).

### Candidates / Applications Pages
- **Archive Dialog (`ArchiveDialogComponent`)**:
  - Add a dropdown for selecting the `RequisitionOpening` when the user selects "Hired".
  - Fetch available openings for the application's job posting via a new service call: `applicationService.getAvailableOpenings(jobPostingId)`.
  - Disable the "Archive" submit button until an opening is selected if resolution is "Hired".
- **Applications Board (`ApplicationsBoardComponent`)**:
  - Update the drop/archive logic to pass the selected `RequisitionOpeningId` when changing status to Archive (Hired).
- **Candidate Detail / Profile (`ApplicationDetailComponent`)**:
  - Display hired details in the application history cards. If an application status is archived and resolution is hired, render a badge: `Hired - [Requisition Title] (Opening #[Seq])`.

## Service Methods
- `RequisitionService`:
  - Add `updateOpening(requisitionId: string, openingId: string, request: { targetStartDate?: string; status?: number })`
- `ApplicationService`:
  - Add `getAvailableOpenings(jobPostingId: string)`
  - Update `updateStatus(id: string, status: number, resolution?: number, requisitionOpeningId?: string)`
