# Application Pipeline — Frontend Specification

### Overview
The Application Pipeline will be implemented as a new feature module in Angular. It uses a Kanban board layout with draggable cards representing candidate applications.

### Dependencies
- `@angular/cdk`: Required for drag-and-drop functionality (`CdkDragDrop`, `CdkDropList`).

### Components

#### `PipelineBoardComponent`
- **Route**: `/admin/pipeline`
- **Responsibilities**:
  - Fetch all active applications (status != Archive).
  - Group applications into three lists: `applied`, `interviewing`, `offered`.
  - Render three `cdkDropList` containers (lanes).
  - Handle `cdkDropListDropped` event to move items between lists and call the backend.
  - Render an "Archive" drop zone at the bottom of the screen.

#### `ApplicationCardComponent` (Sub-component)
- **Inputs**: `application: Application`
- **Responsibilities**:
  - Display candidate name, job title, and applied date.
  - Display a "quick link" to the application detail page.

### Service Integration
Reuse `ApplicationService`:
- `getApplications(jobPostingId?: string)`: To load the initial board state.
- `updateStatus(id: string, status: ApplicationStatus, resolution: ArchivalResolution)`: To persist moves and archival.

### State Management
- Local component state to manage the three lane arrays.
- Optimistic UI updates: Move card immediately, revert if API call fails.

### Routing
Update `app.routes.ts`:
```typescript
{ 
  path: 'admin/pipeline', 
  component: PipelineBoardComponent, 
  canActivate: [authGuard] 
}
```
