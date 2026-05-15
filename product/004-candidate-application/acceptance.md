# Candidate Application — Acceptance Criteria

## Scope
This document covers the public-facing application process for job postings, including form submission, file upload, duplicate prevention, and successful redirection to a confirmation page. Internal dashboard visibility is covered to the extent of verifying the application's existence in the system.

## Prerequisites / Test Setup
- At least one **Published** Job Posting must exist in the system.
- The system must be capable of receiving and storing file uploads (simulated or real).

## Scenarios

### Happy Path
#### AC-01: Successful Application Submission [layer: e2e]
Given a published Job Posting exists
When a candidate fills the application form with a valid Name, Email, Phone, and uploads a PDF resume
And clicks "Submit Application"
Then the application is saved in the system
And the candidate is redirected to a "Thank You" confirmation page
And the application appears in the "Applied" stage of the admin dashboard

### Duplicate Prevention
#### AC-02: Prevent Duplicate Application for Same Job [layer: integration]
Given a candidate with email "test@example.com" has already applied for Job A
When the same candidate tries to apply for Job A again using the same email
Then the system rejects the submission
And an error message "You have already applied for this position" is displayed

#### AC-03: Allow Same Candidate to Apply for Different Jobs [layer: integration]
Given a candidate with email "test@example.com" has already applied for Job A
When the same candidate applies for Job B
Then the application is successfully accepted

### Validation
#### AC-04: Require All Mandatory Fields [layer: integration]
Given a candidate is filling out the application form
When any mandatory field (Name, Email, Resume) is left empty
Then the "Submit" button remains disabled or the system displays an inline validation error for the missing field

#### AC-05: Validate Email Format [layer: integration]
When a candidate enters an invalid email format (e.g., "invalid-email")
Then the system displays an "Invalid email address" error message

### File Upload Constraints
#### AC-06: Reject Non-PDF File Types [layer: integration]
When a candidate attempts to upload a non-PDF file (e.g., .docx, .jpg)
Then the system rejects the file and displays a "Only PDF files are allowed" error message

#### AC-07: Reject Files Exceeding Size Limit [layer: integration]
When a candidate attempts to upload a file larger than 5MB
Then the system rejects the file and displays a "File too large" error message

### Job Availability
#### AC-08: Prevent Application for Closed Job Posting [layer: integration]
Given a Job Posting has been **Closed**
When a candidate tries to submit an application via a direct URL to the form
Then the system returns a 404 Not Found or a message indicating the job is no longer accepting applications.

### Admin Resume Access
#### AC-09: Authenticated Admin Can Download Resume [layer: integration]
Given an application with a PDF resume exists
And the admin is authenticated
When the admin requests to download the resume for that application
Then the system returns the PDF file

#### AC-10: Prevent Unauthorized Resume Download [layer: integration]
Given an application with a PDF resume exists
And the user is NOT authenticated
When the user requests to download the resume for that application via the API
Then the system rejects the request with a 401 Unauthorized error

## Out of Scope
- Candidate account creation (applications are one-off submissions).
- Manual status changes by the admin (covered in Feature 005).
- Email notifications to candidates (deferred).

## Open Questions
- What is the exact maximum file size for resumes? (Assuming 5MB for now).
