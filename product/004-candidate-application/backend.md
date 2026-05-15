# Candidate Application — Backend Design

## Overview
This feature introduces the `Application` entity to store candidate submissions. It includes a public endpoint for submitting applications, multi-part form handling for resume uploads, and business logic to prevent duplicates.

## Database

### New Tables / Changes
**Table: Applications**
- `Id`: Guid, Primary Key
- `JobPostingId`: Guid, Foreign Key to `JobPostings`
- `CandidateName`: String, Required
- `CandidateEmail`: String, Required
- `CandidatePhone`: String
- `ResumePath`: String (Path to the stored file on disk)
- `Status`: Int (Enum: Applied, Interview, Offer, Archive)
- `AppliedAt`: DateTime (UTC)

**Indexes**
- Index on `(JobPostingId, CandidateEmail)` Unique constraint to prevent duplicate applications for the same job.
- Index on `Status` for dashboard filtering.

### EF Core Entities & Configurations
**New Entity: `Application`**
- Belongs to one `JobPosting`.

**New Configuration: `ApplicationConfiguration`**
- Located in `src/Micro.API/Data/Configuration/ApplicationConfiguration.cs`.
- Configures the unique index on `JobPostingId` and `CandidateEmail`.
- Configures required fields and max lengths.

### Migrations
- New migration `AddApplications` to create the table and indexes.

## API Endpoints

### POST `/api/public/jobs/{jobPostingId}/apply`
- **Auth**: None (Public)
- **Request Type**: `multipart/form-data`
- **Fields**:
  - `Name`: String (Required)
  - `Email`: String (Required, Email format)
  - `Phone`: String (Optional)
  - `Resume`: File (Required, Max 5MB, .pdf)
- **Response**:
  - `201 Created`: Application successfully submitted.
  - `400 Bad Request`: Validation failed (missing fields, invalid format, file too large).
  - `404 Not Found`: Job Posting does not exist or is Closed.
  - `409 Conflict`: Candidate has already applied for this job.
- **Notes**: 
  - Implementation must verify `JobPosting.Status == Published`.
  - Resumes will be stored in a `Storage/Resumes/` directory.

### GET `/api/admin/applications`
- **Auth**: Required (Admin)
- **Query Params**: `jobPostingId` (Guid, Optional)
- **Response**: `200 OK` with list of applications.

### GET `/api/admin/applications/{id}/resume`
- **Auth**: Required (Admin)
- **Response**: `200 OK` (File stream) or `404 Not Found`.

## File Storage Strategy
- Files will be saved to `src/Micro.API/wwwroot/uploads/resumes/` (or a configurable path).
- Filenames will be GUID-based to avoid collisions and directory traversal attacks.

## Technical Risks & Dependencies
- **Large File Handling**: Ensure the API handles large multipart requests without timeout or memory exhaustion.
- **File System Permissions**: The API process must have write access to the storage directory.
- **Security**: Validate file magic numbers (signatures) rather than just extensions to prevent malicious uploads.
