# Acceptance Criteria

## Requisition Structure
- AC-01: [layer: integration] Requisition must link to Department, SalaryBand, and CostCenter entities.
- AC-02: [layer: e2e] Requisition with Openings > 1 must allow individual TargetStartDate for each opening.

## Administration
- AC-03: [layer: e2e] Admin can create, edit, and list Departments.
- AC-04: [layer: e2e] Admin can create, edit, and list Salary Bands.
- AC-05: [layer: e2e] Admin can create, edit, and list Cost Centers.

## Business Rules
- AC-06: [layer: integration] SalaryMin and SalaryMax on Requisition must be within the range of the selected SalaryBand.
- AC-07: [layer: integration] Closing an opening does not close the whole Requisition unless it is the last one.
