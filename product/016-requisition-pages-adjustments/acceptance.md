# Acceptance Criteria - Requisition Pages Adjustments

## Requisition List and Details

- AC-01: [layer: e2e] Requisition list displays the ratio of filled openings (e.g., "1 / 3") instead of just a static total count.
- AC-02: [layer: e2e] A finalized or closed Requisition page displays a read-only list of all openings, showing their sequence number, target start date, status, and the name of the hired candidate (with a link to their profile) if filled.
- AC-03: [layer: e2e] A user can edit the target start date of an unfilled opening on a finalized requisition.
- AC-04: [layer: e2e] A user can cancel an unfilled opening on a finalized requisition, which updates its status to Cancelled.
- AC-05: [layer: e2e] Creating or editing a draft requisition with openings count > 1 allows setting a separate Target Start Date for each opening in the form.

## Candidate Archival and Hiring

- AC-06: [layer: e2e] Archiving a candidate with resolution `Hired` requires choosing an available opening from the corresponding requisition.
- AC-07: [layer: integration] When an application is successfully archived with resolution `Hired` and an opening is selected:
  - The application is linked to the opening.
  - The opening status transitions to `Filled`, and its candidate reference is set to the application's candidate.
- AC-08: [layer: integration] When all openings of a requisition are filled or cancelled, the requisition status changes to `Closed` and the associated Job Posting status transitions to `Closed`.
- AC-09: [layer: e2e] A hired candidate's profile page displays the details of the opening they were hired for (e.g., "Hired into [Requisition Title], Opening #[Number]").
