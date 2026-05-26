# 019: Selectable Custom Fields — Acceptance Criteria

## Field Definition Configuration

**AC-01** `[layer: integration]`
Given admin creates a new custom field (e.g., ShortText) and sets "Global" to false,
when saved,
then the field is added to the pool as a Selectable field and does not automatically appear on any forms.

**AC-02** `[layer: integration]`
Given a Selectable custom field exists in the pool,
when a user creates a new Requisition or Job Posting,
then the new instance does not display the Selectable field by default (only Global fields are rendered).

---

## Requisition Association

**AC-03** `[layer: e2e]`
Given a requisition exists,
when Hiring Manager views the Requisition's custom field configuration tab and selects a Selectable field from the pool,
then the field is successfully associated with that specific Requisition.

**AC-04** `[layer: integration]`
Given a Requisition has a selected custom field associated with it,
when a user opens the edit form for that Requisition,
then both the Global custom fields and the selected custom field are rendered and validated upon submission.

**AC-05** `[layer: integration]`
Given a Requisition has a selected custom field associated with it,
when a user opens the edit form for a different Requisition that does not have this field associated,
then the selected custom field is not rendered.

---

## Job Posting & Application Association

**AC-06** `[layer: e2e]`
Given a Job Posting exists,
when Recruiter edits the Job Posting and adds a Selectable Job Posting field AND a Selectable Application field from the pool,
then both fields are successfully associated with that specific Job Posting.

**AC-07** `[layer: integration]`
Given a Job Posting has an associated Selectable Application field (marked candidate-facing),
when a candidate visits the public application form for this specific Job Posting,
then the form renders the selected application field alongside any global candidate-facing fields.

**AC-08** `[layer: integration]`
Given two Job Postings (Job A with associated selectable application field, Job B without it),
when a candidate visits the public application form for Job B,
then the selected application field from Job A is not rendered.

---

## Unlinking and Safeguards

**AC-09** `[layer: integration]`
Given a Requisition has an associated selectable custom field,
when the user removes the association and no values have been saved for this field on this Requisition,
then the association is deleted and the field ceases to appear on the Requisition.

**AC-10** `[layer: integration]`
Given a Requisition has an associated selectable custom field and a user has already saved a value for this field on the Requisition,
when the user attempts to remove the association,
then the request is rejected with a validation error and the association is preserved.

**AC-11** `[layer: integration]`
Given a custom field definition is set to "Global",
when the admin attempts to change it to "Selectable" (or vice-versa) and there are already saved values for this field on any entity,
then the modification is rejected.
