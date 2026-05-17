# Application Pipeline — Backend Specification

### Overview
The Kanban board functionality leverages existing application management endpoints. No schema changes or new API endpoints are required to support the basic drag-and-drop status transitions.

### API Contracts

#### Update Application Status
Used when a candidate card is moved between lanes or archived.

- **Endpoint**: `PUT /api/admin/applications/{id}/status`
- **Request Body**:
  ```json
  {
    "status": 1, // Applied=0, Interview=1, Offer=2, Archive=3
    "resolution": 0 // None=0, Hired=1, Rejected=2, Declined=3, Withdrawn=4
  }
  ```
- **Response**: `204 No Content`

### Entity Considerations
- **Application**: The `Status` and `ArchivalResolution` fields are updated. `UpdatedAt` should be set to current UTC time.
- **Feedback**: Moving stages does not automatically create feedback entries in the current implementation, but the frontend may choose to trigger a feedback modal if required in future iterations.
