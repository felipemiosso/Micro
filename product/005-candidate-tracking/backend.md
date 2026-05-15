# Candidate Tracking — Backend Design

## Overview
This feature extends the `Application` entity to support stage progression and introduces a new `Feedback` entity. It also adds endpoints for managing candidate status and recording assessments.

## Database

### New Tables / Changes
**New Table: Feedbacks**
- `Id`: Guid, Primary Key
- `ApplicationId`: Guid, Foreign Key to `Applications`
- `AdminId`: String (Identity User Id)
- `Notes`: String, Required
- `Score`: Int (1-5)
- `CreatedAt`: DateTime (UTC)

**Changes to Table: Applications**
- `ArchivalResolution`: Int (Enum: None, Hired, Rejected, Declined, Withdrawn)
- `UpdatedAt`: DateTime (UTC)

### EF Core Entities & Configurations
**New Entity: `Feedback`**
- Belongs to one `Application`.

**Configuration: `FeedbackConfiguration`**
- Located in `src/Micro.API/Data/Configuration/FeedbackConfiguration.cs`.
- `Notes` max length: 2000.
- `Score` range validation: 1-5.

**Updated Entity: `Application`**
- Add `ICollection<Feedback> Feedbacks`.
- Add `ArchivalResolution`.

### Migrations
- New migration `AddFeedbackAndArchival` to create the `Feedbacks` table and update `Applications`.

## API Endpoints

### PUT `/api/admin/applications/{id}/status`
- **Auth**: Required (Admin)
- **Request Body**:
  ```json
  {
    "status": 1, // ApplicationStatus
    "resolution": 0 // ArchivalResolution (Required if status is Archive)
  }
  ```
- **Response**: `204 No Content`, `400 Bad Request` (if Archive status is missing resolution).

### POST `/api/admin/applications/{id}/feedback`
- **Auth**: Required (Admin)
- **Request Body**:
  ```json
  {
    "notes": "string",
    "score": 5
  }
  ```
- **Response**: `201 Created`.

### GET `/api/admin/applications/{id}/history`
- **Auth**: Required (Admin)
- **Response**: `200 OK` with a combined list of stage changes (derived or tracked) and feedback entries.

## Technical Risks & Dependencies
- **Data Integrity**: Ensure that moving to `Archive` requires a resolution status at the database level if possible (or via API validation).
- **Concurrency**: Multiple admins might try to update the same candidate simultaneously.
