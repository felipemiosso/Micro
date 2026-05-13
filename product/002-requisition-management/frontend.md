# Frontend Design - Requisition Management

## Components

### RequisitionListComponent
- **Route:** `/admin/requisitions`
- **Template:** A table showing Title, Dept, Openings, and Status.
- **Actions:** 
  - "New Requisition" button.
  - "Edit" link (if status == Draft).
  - "Finalize" button (if status == Draft).
  - "Close" button (if status == Finalized).

### RequisitionFormComponent
- **Route:** `/admin/requisitions/new` or `/admin/requisitions/edit/:id`
- **Template:** Reactive form with inputs for Title, Department, and Openings.
- **Logic:** Handles both creation and updates.

## Service

### RequisitionService
- `getAll()`: `Observable<Requisition[]>`
- `getById(id)`: `Observable<Requisition>`
- `create(data)`: `Observable<void>`
- `update(id, data)`: `Observable<void>`
- `finalize(id)`: `Observable<void>`
- `close(id)`: `Observable<void>`

## Routing
- Add `/admin/requisitions` and children to `app.routes.ts`, protected by `AuthGuard`.
