# Candidate Tracking — Acceptance Criteria

## Scope
This document covers the internal management of candidate applications by administrators, including stage progression, feedback recording, search/filter functionality, and archival with resolution statuses.

## Prerequisites / Test Setup
- Admin user is authenticated.
- Multiple candidates exist in various stages (Applied, Interview, etc.).
- Multiple job postings exist to test filtering.

## Scenarios

### Applications Overview
#### AC-01: Filter Candidates by Job Posting [layer: e2e]
Given multiple candidates exist for "Job A" and "Job B"
When the admin selects "Job A" from the job filter
Then only candidates who applied for "Job A" are displayed in the list

#### AC-02: Search Candidates by Name/Email [layer: e2e]
Given a candidate named "John Doe" with email "john@example.com" exists
When the admin searches for "John" or "example.com"
Then "John Doe" is displayed in the search results

### Stage Management
#### AC-03: Move Candidate to Next Stage [layer: integration]
Given a candidate is in the "Applied" stage
When the admin changes their stage to "Interview"
Then the candidate's stage is updated to "Interview"
And the change is reflected in the applications board

#### AC-04: Archive Candidate with Resolution [layer: e2e]
Given a candidate is in the "Offer" stage
When the admin moves the candidate to the "Archive" stage
And selects the resolution "Hired"
Then the candidate is moved to "Archive"
And their archival resolution is recorded as "Hired"

### Feedback & Evaluation
#### AC-05: Record Feedback and Score [layer: integration]
Given an admin is viewing a candidate's profile
When the admin adds a note "Excellent technical skills" and a score of 5
Then the feedback is saved
And it is displayed in the candidate's feedback history

#### AC-06: View Candidate History [layer: e2e]
Given a candidate has been moved through multiple stages and has several feedback entries
When the admin views the candidate's details
Then a chronological history of all stage changes and feedback notes is displayed

## Out of Scope
- Automated emails to candidates (manual "Send Email" action is a placeholder/future feature).
- Bulk actions (moving multiple candidates at once).
- Custom stage creation (stages are fixed: Applied, Interview, Offer, Archive).

## Open Questions
- Should we allow editing/deleting feedback entries once they are saved? (Assuming read-only history for now).
