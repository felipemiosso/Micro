# Candidate Profile Separation — Acceptance Criteria

### Candidate Creation & Linking
- **AC-01 [layer: integration]**: New candidate created on first application.
  - **Given** no candidate exist with email "new@test.com"
  - **When** person apply for "Job X" with that email
  - **Then** new record in `Candidate` table created
  - **And** `Application` link to that `Candidate`.

- **AC-02 [layer: integration]**: Existing candidate linked on repeat application.
  - **Given** candidate "John Doe" (john@test.com) exist
  - **When** person apply for "Job Y" with email "john@test.com"
  - **Then** no new `Candidate` created
  - **And** new `Application` link to existing `Candidate` record.

### Independent Progress
- **AC-03 [layer: e2e]**: Applications have independent stages.
  - **Given** candidate "Jane" has application for "Job A" in "Interview"
  - **When** Jane apply for "Job B"
  - **Then** "Job B" application status is "Applied"
  - **And** "Job A" status remains "Interview".

### Profile Synchronization
- **AC-04 [layer: e2e]**: Profile update reflect across applications.
  - **Given** candidate has applications for "Job A" and "Job B"
  - **When** admin update candidate phone number in "Job A" detail view
  - **Then** candidate phone number update in "Job B" detail view too.
