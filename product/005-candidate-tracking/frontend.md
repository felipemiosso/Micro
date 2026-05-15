# Candidate Tracking — Frontend Design

## Overview
This feature adds a candidate details view, a feedback form, and status management controls to the admin dashboard.

## Components

### ApplicationDetailComponent
- **Name**: `ApplicationDetailComponent`
- **Route**: `/admin/candidates/:id`
- **Responsibility**: Displays candidate contact info, resume preview link, and hiring stage progression. Shows full feedback history.
- **Template outline**:
  - Profile header with Name, Email, Job Title.
  - Stage selection dropdown or button group.
  - Archival modal (if "Archive" is selected).
  - Feedback list (chronological).
  - "Add Feedback" section.

### FeedbackFormComponent
- **Name**: `FeedbackFormComponent` (Child of ApplicationDetail)
- **Responsibility**: Renders a textarea for notes and a star-rating or numeric input for the 1-5 score.

## Services

### ApplicationService (Updated)
- **Methods**:
  - `getApplication(id: string): Observable<ApplicationDetail>`
  - `updateStatus(id: string, status: ApplicationStatus, resolution?: ArchivalResolution): Observable<void>`
  - `addFeedback(id: string, data: FeedbackRequest): Observable<void>`

## TypeScript Interfaces

```typescript
export enum ArchivalResolution {
  None = 0,
  Hired = 1,
  Rejected = 2,
  Declined = 3,
  Withdrawn = 4
}

export interface Feedback {
  id: string;
  notes: string;
  score: number;
  adminName: string;
  createdAt: string;
}

export interface ApplicationDetail extends Application {
  archivalResolution: ArchivalResolution;
  feedbacks: Feedback[];
}
```

## Routing
- `/admin/candidates` -> Existing list, updated with "View" button.
- `/admin/candidates/:id` -> `ApplicationDetailComponent`.

## State & Data Flow
1. **View Detail**: Admin clicks a candidate in the list.
2. **Fetch Data**: `ApplicationService.getApplication()` retrieves full profile including feedback.
3. **Change Status**: Admin selects new status → API call → UI updates stage badge.
4. **Add Feedback**: Admin submits note/score → API call → Feedback list prepends new entry.

## Error & Edge Case Handling
- **Archival Validation**: Prevent moving to Archive without selecting a resolution in the UI.
- **Score Range**: UI should restrict score input to 1-5.

## Technical Risks & Dependencies
- **History View**: Combining stage changes and feedback notes into a single unified timeline for a better UX.
