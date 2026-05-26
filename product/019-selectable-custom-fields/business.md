# 019: Selectable Custom Fields — Business Specification

## Overview

Currently, custom fields defined by Admins are "Global" — they automatically apply to every single Requisition, Job Posting, or Application in the system. While this works for fields that are universally required, it clutters the interface for fields that are only relevant to specific positions, departments, or postings. 

For example, a "Has Forklift License?" question is only relevant to warehouse job applications, and a "GitHub Profile" is only relevant to software engineering positions.

To solve this, we are introducing **Selectable Custom Fields**. 
1. **Admins** define custom fields in the central pool and decide if they are **Global** or **Selectable**.
   * **Global**: Automatically active on all instances of the target entity.
   * **Selectable**: Created as a template in the pool. It does not apply to any instances automatically.
2. **Hiring Managers and Recruiters** can select and associate these optional/selectable fields to their specific **Requisitions** or **Job Postings** (including candidate-facing application fields linked to those job postings).

This division of roles keeps the custom field configuration clean and ensures that users only fill out fields relevant to their context.

---

## User Stories

### Field Configuration & Pool (Admin)
* As an Admin, I want to create a custom field and mark it as `Selectable` so that it doesn't automatically show on all requisitions or job postings.
* As an Admin, I want to view all custom fields in the settings panel and see at a glance which ones are `Global` and which ones are `Selectable` templates.
* As an Admin, I want to edit and disable selectable custom fields in the pool just like global custom fields.

### Associating Fields (Hiring Managers / Recruiters)
* As a Hiring Manager/Recruiter editing a Requisition, I want to browse the pool of selectable Requisition custom fields and add them to my requisition.
* As a Recruiter editing a Job Posting, I want to browse the pool of selectable Job Posting custom fields and add them to my job posting.
* As a Recruiter editing a Job Posting, I want to browse the pool of selectable Application custom fields (Global or stage-specific) and associate them with my job posting so that candidates applying to this posting (or recruiters evaluating applications for it) are prompted to fill them in.
* As a user, I want to remove/unlink a selected custom field from my Requisition or Job Posting if no data has been collected for it yet, to correct mistakes.
* As a user, I want to see the list of active custom fields (both Global and selected) on my Requisition or Job Posting form.

### Data Entry & Visibility (Candidates & Recruiters)
* As a Candidate, when I apply to a specific Job Posting, I want the form to render the global candidate-facing fields AND any selectable candidate-facing fields that the recruiter associated with this specific Job Posting.
* As a Recruiter, when viewing an Application, I want to see the values for the global application custom fields AND the selectable application custom fields linked to the application's Job Posting.

---

## Open Questions

### Q-01: What happens if a user tries to remove a selectable field that already has collected values?
* **Resolution**: The system must block removing/unlinking a custom field from an entity instance (Requisition/Job Posting) if any value exists for that field on that instance (or on associated applications). This prevents accidental data loss. Users can disable the field globally if they want to retire it, or we can offer a warning, but blocking is the safest default behavior.

### Q-02: Can a field's "Global" vs "Selectable" setting be changed after creation?
* **Resolution**: To prevent complex database migrations and integrity issues, the `IsGlobal` setting is immutable after creation if the field already has recorded values. If the field has no values, it can be updated.
