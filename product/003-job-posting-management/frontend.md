# Frontend Design - Job Posting Management

## Components

### Admin Area

#### JobPostingListComponent
- **Selector:** `app-job-posting-list`
- **Purpose:** Display a table of all job postings with their status and parent requisition title.
- **Actions:** Navigate to edit, manually close a posting.

#### JobPostingEditComponent
- **Selector:** `app-job-posting-edit`
- **Purpose:** Edit the public title, description, and requirements of a job posting.
- **Actions:** Save changes, cancel.

### Public Area

#### JobBoardComponent
- **Selector:** `app-job-board`
- **Purpose:** Display a list of all "Published" job postings as cards or a list.
- **Navigation:** Click on a job to see details.

#### JobDetailComponent
- **Selector:** `app-job-detail`
- **Purpose:** Display full details of a job posting.
- **Content:** Title, Department (via parent requisition), Description, Requirements.
- **Action:** "Apply Now" button (currently a placeholder or link to Feature 004).

## Service Contract

### JobPostingService
```typescript
@Injectable({ providedIn: 'root' })
export class JobPostingService {
  // Public APIs
  getPublishedJobs(): Observable<JobPosting[]>;
  getJobById(id: string): Observable<JobPosting>;

  // Admin APIs
  getAllJobs(): Observable<JobPosting[]>;
  updateJob(id: string, data: Partial<JobPosting>): Observable<void>;
  closeJob(id: string): Observable<void>;
}
```

## Routing

### Public Routes
- `/jobs` -> `JobBoardComponent`
- `/jobs/:id` -> `JobDetailComponent`

### Admin Routes
- `/admin/jobs` -> `JobPostingListComponent`
- `/admin/jobs/edit/:id` -> `JobPostingEditComponent`

## State Management
- Use simple signals or local component state for job data.
- Fetch data on component initialization via `JobPostingService`.
