# Candidate Profile Separation — Frontend Specification

### Model Changes
Update `Application` and `ApplicationDetail` interfaces in `application.service.ts`:

```typescript
export interface Candidate {
  id: string;
  fullName: string;
  email: string;
  phone?: string;
}

export interface Application {
  id: string;
  jobPostingId: string;
  jobTitle: string;
  candidateId: string;
  candidate: Candidate; // Nested profile
  status: ApplicationStatus;
  appliedAt: string;
}
```

### UI Components

#### `ApplicationDetailComponent`
- Show data from `app.candidate.fullName`, `app.candidate.email`, etc.
- Add "Edit Profile" mode to update `Candidate` info.
- List "Other Applications" section to show history for this candidate.

#### `ApplicationCardComponent`
- Bind to `application.candidate.fullName`.

### Service Updates
- `updateCandidate(id: string, data: Partial<Candidate>)`: New method for profile edits.
- Update mapping logic if backend response structure change.
