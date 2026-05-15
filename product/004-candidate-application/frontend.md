# Candidate Application — Frontend Design

## Overview
This feature adds a public application form to the job detail page, allowing candidates to submit their information and resumes. It also includes a confirmation "Thank You" page.

## Components

### ApplicationFormComponent
- **Name**: `ApplicationFormComponent`
- **Route**: Embedded in `JobDetailComponent` or as a child route `/jobs/:id/apply`.
- **Responsibility**: Renders a reactive form for candidate details and file upload. Handles validation and submission.
- **Template outline**:
  - Input fields for Name, Email, Phone.
  - File input for Resume (PDF only).
  - Submit button (disabled if invalid).
  - Error messages for duplicates or validation failures.

### ApplicationSuccessComponent
- **Name**: `ApplicationSuccessComponent`
- **Route**: `/jobs/apply/success`
- **Responsibility**: Displays a success message and a link back to the job board.

## Services

### ApplicationService
- **Name**: `ApplicationService`
- **Responsibility**: Handles API calls related to applications.
- **Methods**:
  - `apply(jobId: string, data: FormData): Observable<void>`

## TypeScript Interfaces

```typescript
export enum ApplicationStatus {
  Applied = 0,
  Interview = 1,
  Offer = 2,
  Archive = 3
}

export interface Application {
  id: string;
  jobPostingId: string;
  candidateName: string;
  candidateEmail: string;
  candidatePhone?: string;
  appliedAt: string;
  status: ApplicationStatus;
}
```

## Routing
- `/jobs/:id` -> Displays Job Detail + "Apply" button.
- `/jobs/:id/apply` -> Displays the application form.
- `/jobs/apply/success` -> Displays the confirmation page.

## State & Data Flow
1. **Load Job Detail**: User navigates to `/jobs/:id`.
2. **Open Form**: User clicks "Apply Now" (navigates to `/jobs/:id/apply`).
3. **Submit**: Form data (including `File` object) is wrapped in `FormData` and sent via `ApplicationService.apply()`.
4. **Loading**: Submit button shows a spinner.
5. **Success**: Redirect to `/jobs/apply/success`.
6. **Error**: Display API error message (e.g., "Duplicate email").

## Error & Edge Case Handling
- **File Validation**: Immediate feedback if the file is too large or has an invalid extension.
- **API Errors**: Handle `409 Conflict` (duplicate) and `404 Not Found` (closed job) by showing user-friendly messages.
- **Loading State**: Disable form submission during the upload process.

## Technical Risks & Dependencies
- **FormData with HttpClient**: Ensure proper headers are (not) set so the browser correctly adds the multipart boundary.
- **Resume File Size**: Client-side validation to prevent uploading huge files before they reach the server.
