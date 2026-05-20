# Requisition Overhaul

Drastic change to Requisitions. Support multiple openings with individual overrides. Normalize Department, Salary Band, and Cost Center.

## User Stories

- As Admin, I want to manage Departments, Salary Bands, and Cost Centers in a unified dashboard.
- As Hiring Manager, I want to create a Requisition for multiple openings.
- As Recruiter, I want to set different target dates for each opening in a single Requisition.
- As Recruiter, I want to track which opening a candidate is assigned to.
- As System, I want to ensure Salary is within the selected Salary Band.

## Decisions

- **Data Migration**: None. Drop and recreate database.
- **Opening Status**: Each opening is independent. Can be closed individually.
- **Application Flow**: Applicants apply to Job Posting. Recruiter assigns to specific Opening during hiring.
