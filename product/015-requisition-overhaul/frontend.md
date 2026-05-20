# Technical Design - Frontend

## Components

### Requisition Form Refactor
- Update `requisition-form.ts` to use dropdowns for Department, Salary Band, and Cost Center.
- Fetch data from new services (`DepartmentService`, `SalaryBandService`, `CostCenterService`).
- Dynamic section for Openings: if `Openings > 1`, show a list to set individual `TargetStartDate`.

### Admin Dashboard
- Unified page `/admin/settings` with tabs for:
  - Departments
  - Salary Bands
  - Cost Centers
- Shared layout, consistent table actions.

## State Management
- Use Signals for lookup lists.
- `RequisitionDetail` signal should include the nested `Openings` array.

## UI Standards
- Use `NotificationService` for save success/error.
- Use `ConfirmDialog` for deleting lookup entries.
- Table headers must match the new streamlined style.
