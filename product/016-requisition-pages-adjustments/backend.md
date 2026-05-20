# Technical Design - Backend

## API Contracts

### Requisition Extensions
- `GET /api/requisitions` and `GET /api/requisitions/{id}`:
  - Return the nested collection of `Openings` in the JSON response payload.
  - For each opening, include:
    - `Guid Id`
    - `int SequenceNumber`
    - `DateTime? TargetStartDate`
    - `Guid? CandidateId`
    - `string? CandidateName` (resolved via Candidate reference if filled)
    - `OpeningStatus Status` (Open, Filled, Cancelled)

- `POST /api/requisitions` / `PUT /api/requisitions/{id}`:
  - Accept an optional array of openings containing custom target start dates in the requests:
    ```csharp
    public record RequisitionOpeningDto(int SequenceNumber, DateTime? TargetStartDate);
    ```
  - When saving a draft, create or update `RequisitionOpening` records in the database with status `Open`.
  - Modify `FinalizeRequisition` to NOT construct openings from scratch, as they are already initialized during draft updates/creation.

- `PUT /api/requisitions/{id}/openings/{openingId}`:
  - Endpoint to update an individual opening's details.
  - Body:
    ```csharp
    public record UpdateRequisitionOpeningRequest(DateTime? TargetStartDate, OpeningStatus Status);
    ```
  - Rules:
    - Only `TargetStartDate` and `Status` can be updated.
    - Status can only be changed to `Cancelled` from `Open` (cannot cancel a `Filled` opening, cannot modify a `Cancelled` opening).

### Application Extensions
- `GET /api/job-postings/{jobPostingId}/available-openings`:
  - Returns a list of `RequisitionOpening` records associated with the requisition of the job posting that are currently in `Open` status.

- `PUT /api/applications/{id}/status`:
  - Body updated to include optional `Guid? RequisitionOpeningId`:
    ```csharp
    public record UpdateStatusRequest(ApplicationStatus Status, ArchivalResolution Resolution, Guid? RequisitionOpeningId);
    ```
  - Rules:
    - If `Status == Archive` and `Resolution == Hired`:
      - `RequisitionOpeningId` is REQUIRED.
      - Validate that the opening exists, is associated with the correct requisition, and is `Open`.
      - In a database transaction:
        1. Set `Application.RequisitionOpeningId = RequisitionOpeningId`.
        2. Set `RequisitionOpening.CandidateId = Application.CandidateId`.
        3. Set `RequisitionOpening.Status = OpeningStatus.Filled`.
        4. If all openings of the requisition are now `Filled` or `Cancelled`:
           - Set `Requisition.Status = RequisitionStatus.Closed` and `Requisition.ClosedAt = DateTime.UtcNow`.
           - Set associated `JobPosting.Status = JobPostingStatus.Closed` and `JobPosting.ClosedAt = DateTime.UtcNow`.

- `GET /api/candidates/{id}`:
  - In `CandidateApplicationResponse`, return:
    - `Guid? RequisitionOpeningId`
    - `int? OpeningSequenceNumber`
    - `string? RequisitionTitle`
