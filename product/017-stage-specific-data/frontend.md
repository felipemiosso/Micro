# 017: Stage-Specific Application Data - Frontend Design

## State & Service Contracts
- Update `Application` model interface in `application.service.ts` to include `interviewDetails` and `offerDetails`.
- Add service methods `updateInterviewDetails(appId, payload)` and `updateOfferDetails(appId, payload)`.

## Component Design
### Candidate Profile (`application-detail.html`)
Refactor the right-side management panel to be context-aware based on `app.status`. Use Angular control flow (`@switch` or `@if`) to render different content blocks based on the current stage.

- **Interview View**: 
  - Show fields for Interviewer Name and Date.
  - Visually group the existing Feedback component under the Interview stage assessment.
- **Offer View**:
  - Show a dedicated form for Offer Details: Salary (number input with currency symbol prefix), Target Start Date (datepicker), Deadline (datepicker).
  - Include an "Update Offer Details" button.
- **History/Summary View**:
  - Display summary cards for completed stages. If the candidate is in the Offer stage, show a read-only summary of the Interview details above the Offer form.

## Design System Usage
- Use `.input-honeycomb` for all new text, number, and date inputs.
- Use `.btn-primary` and `.btn-secondary` consistently for form submissions.
- Ensure forms are enclosed in distinct sections (`.bg-slate-50`, rounded borders) to distinguish stage data visually from the main candidate header.
