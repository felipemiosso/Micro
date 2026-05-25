# 018: Custom Fields — Acceptance Criteria

## Field Definition Management

### Happy Path — Create

**AC-01** `[layer: integration]`
Given admin opens Custom Fields for Requisition and creates a ShortText field with a label and no validation rules,
when saved,
then the field appears in the Active Fields list and immediately renders in the Requisition create and edit forms.

**AC-02** `[layer: integration]`
Given admin creates a Number field with minimum 0 and maximum 1000000,
when saved,
then the Requisition form renders a numeric input for that field and rejects values outside the range.

**AC-03** `[layer: integration]`
Given admin creates a Date field with an earliest date of 2025-01-01 and no latest date,
when saved,
then the Requisition form renders a date picker and rejects dates before 2025-01-01.

**AC-04** `[layer: integration]`
Given admin creates a Yes / No field,
when saved,
then the Requisition form renders a boolean toggle for that field.

**AC-05** `[layer: integration]`
Given admin creates a Single Choice field with options Júnior, Pleno, Sênior,
when saved,
then the Requisition form renders a dropdown containing exactly those three options.

**AC-06** `[layer: integration]`
Given admin creates a ShortText field and selects the CPF preset,
when a user submits a value that has the correct CPF format but a failing check digit,
then the save is rejected with a field-level error message.

**AC-07** `[layer: integration]`
Given admin creates a ShortText field and selects both CPF and CNPJ presets,
when a user submits a valid CPF,
then the field is accepted.

**AC-08** `[layer: integration]`
Given admin creates a ShortText field and selects both CPF and CNPJ presets,
when a user submits a valid CNPJ,
then the field is accepted.

**AC-09** `[layer: integration]`
Given admin creates a ShortText field and selects both CPF and CNPJ presets,
when a user submits a value that is neither a valid CPF nor a valid CNPJ,
then the save is rejected with a field-level error message.

**AC-10** `[layer: integration]`
Given admin creates a ShortText field with a format mask of `DEPT-####`,
when a user submits a value that does not match that shape,
then the save is rejected with a field-level error message.

**AC-11** `[layer: integration]`
Given admin creates a ShortText field with a format mask of `DEPT-####`,
when a user submits `DEPT-0042`,
then the field is accepted.

**AC-12** `[layer: integration]`
Given admin marks a new Application (Applied) field as candidate-facing,
when a candidate visits the public application form for any published job posting,
then that field is rendered in the form.

**AC-13** `[layer: e2e]`
Given admin opens the preset selector when configuring a ShortText field,
then presets are displayed grouped under the tags Documentos, Contato, and Localização.

**AC-14** `[layer: integration]`
Given admin selects a named preset on a ShortText field,
then the format mask input is hidden and unavailable.

---

### Ordering

**AC-15** `[layer: integration]`
Given an entity already has three active custom fields,
when admin creates a fourth field,
then the new field appears at the bottom of the Active Fields list.

**AC-16** `[layer: e2e]`
Given admin reorders custom fields by drag-and-drop,
then the new order is reflected immediately in the entity's create and edit forms.

---

### Edit — Safe Changes

**AC-17** `[layer: integration]`
Given a field has recorded values,
when admin changes only the field label,
then the change is saved and the new label is displayed everywhere the field appears.

**AC-18** `[layer: integration]`
Given a field has recorded values,
when admin changes only the help text,
then the change is saved and the new help text appears below the input.

**AC-19** `[layer: integration]`
Given a field has recorded values,
when admin increases the maximum character length,
then the change is saved.

**AC-20** `[layer: integration]`
Given a field has recorded values,
when admin adds a new option to a Single Choice field,
then the new option appears in the dropdown for future entries.

**AC-21** `[layer: integration]`
Given a field is currently Required,
when admin sets it to optional,
then existing records with a blank value can be saved without filling the field.

---

### Edit — Blocked Changes When Values Exist

**AC-22** `[layer: integration]`
Given a ShortText field has stored values and admin attempts to decrease the maximum character length to a value that at least one stored value exceeds,
then the change is rejected and admin is shown the count of records that would be affected.

**AC-23** `[layer: integration]`
Given a ShortText field had no maximum length and admin attempts to add one that at least one stored value would exceed,
then the change is rejected and admin is shown the count of affected records.

**AC-24** `[layer: integration]`
Given a Single Choice field has stored values and admin attempts to remove an option that is stored in at least one record,
then the removal is rejected and admin is informed that the option is in use.

**AC-25** `[layer: integration]`
Given a field has stored values,
when admin attempts to change the field type,
then the change is rejected with a plain-language explanation.

**AC-26** `[layer: integration]`
Given a ShortText field has stored values and admin attempts to change the named preset,
then the change is rejected with a plain-language explanation.

**AC-27** `[layer: integration]`
Given a ShortText field has stored values and admin attempts to change the format mask,
then the change is rejected with a plain-language explanation.

**AC-28** `[layer: integration]`
Given a Single Choice field has an option that is not stored in any record,
when admin removes that option,
then the removal is accepted and the option no longer appears in the dropdown.

**AC-29** `[layer: integration]`
Given a ShortText field has a maximum length of 100 and all stored values have fewer than 20 characters,
when admin decreases the maximum to 20,
then the change is accepted.

---

### Making a Field Required

**AC-30** `[layer: integration]`
Given admin makes an optional field Required,
when at least one existing record has no value for that field,
then admin is shown the count of records that will be affected on their next save.

**AC-31** `[layer: integration]`
Given a field was optional and has been made Required,
when a user opens an existing record that has no value for that field and saves without filling it,
then the save is rejected with a field-level error on that custom field.

---

## Disable and Re-enable

**AC-32** `[layer: integration]`
Given an active field has 47 recorded values,
when admin disables it,
then the field no longer appears in create or edit forms for that entity.

**AC-33** `[layer: integration]`
Given a field has been disabled,
when a user views an existing record that had a value for that field,
then the value is displayed in read-only mode with a "Campo desativado" indicator.

**AC-34** `[layer: integration]`
Given a disabled field has recorded values,
when admin re-enables it,
then the field reappears in create and edit forms and all previously recorded values are visible again.

**AC-35** `[layer: integration]`
Given admin re-enables a field,
then the field's record count is the same as it was before the field was disabled.

---

## Deletion

**AC-36** `[layer: integration]`
Given a field has zero recorded values,
when admin clicks Delete and confirms,
then the field is permanently removed and no longer appears anywhere in the system.

**AC-37** `[layer: integration]`
Given a field has one or more recorded values,
when admin clicks Delete,
then no delete confirmation is shown; instead an informational message is shown explaining that the data would be lost, and only a Disable option is offered.

**AC-38** `[layer: integration]`
Given a field has one or more recorded values,
when a direct API call attempts to hard-delete it,
then the request is rejected with an error indicating the field has recorded values.

---

## End-User Data Entry — Validation

### Required

**AC-39** `[layer: integration]`
Given a Requisition has an active Required custom field,
when a user saves the Requisition without providing a value for that field,
then the save is rejected and a field-level error is shown inline next to the field.

**AC-40** `[layer: integration]`
Given a Requisition has an active Required custom field,
when a user saves the Requisition with a value provided,
then the save succeeds.

### Text Length

**AC-41** `[layer: integration]`
Given a ShortText field has a minimum of 10 characters,
when a user submits a value with fewer than 10 characters,
then the save is rejected with a field-level error.

**AC-42** `[layer: integration]`
Given a ShortText field has a maximum of 50 characters,
when a user submits a value with more than 50 characters,
then the save is rejected with a field-level error.

### Number Constraints

**AC-43** `[layer: integration]`
Given a Number field has a minimum of 0,
when a user submits -1,
then the save is rejected with a field-level error.

**AC-44** `[layer: integration]`
Given a Number field has a maximum of 1000,
when a user submits 1001,
then the save is rejected with a field-level error.

**AC-45** `[layer: integration]`
Given a Number field,
when a user submits a non-numeric value,
then the save is rejected with a field-level error.

### Date Constraints

**AC-46** `[layer: integration]`
Given a Date field has an earliest date of 2025-01-01,
when a user submits 2024-12-31,
then the save is rejected with a field-level error.

**AC-47** `[layer: integration]`
Given a Date field has a latest date of 2025-12-31,
when a user submits 2026-01-01,
then the save is rejected with a field-level error.

### Single Choice

**AC-48** `[layer: integration]`
Given a Single Choice field with options A, B, C,
when the backend receives a submission with value D,
then the save is rejected with a field-level error.

---

## Retroactive Validation

**AC-49** `[layer: integration]`
Given a record has a stored custom field value that now violates a currently active rule (e.g. the field was made Required after the record was created with no value),
when a user opens that record, changes an unrelated standard field, and saves,
then the save is rejected with a field-level error pointing to the custom field that is now in violation.

**AC-50** `[layer: integration]`
Given a backend validation fails on a custom field value that the user did not touch in the current session,
then the error message identifies the specific field by its label so the user knows what to correct.

---

## Application Stage Scoping

**AC-51** `[layer: integration]`
Given a custom field is scoped to Application — Applied,
when viewing an application in the Applied stage,
then the field is visible and editable.

**AC-52** `[layer: integration]`
Given a custom field is scoped to Application — Applied,
when viewing an application in the Interview stage,
then the field is not visible in the edit form.

**AC-53** `[layer: integration]`
Given a custom field is scoped to Application — Interview,
when viewing an application in the Applied stage,
then the field is not visible in the edit form.

**AC-54** `[layer: integration]`
Given a custom field is scoped to Application — Global,
when viewing an application in any stage (Applied, Interview, Offer),
then the field is visible and editable.

---

## Candidate-Facing Fields

**AC-55** `[layer: integration]`
Given an Application (Applied) custom field is marked candidate-facing,
when a candidate submits the public application form with a value for that field,
then the value is stored and visible to internal users on the Candidate Profile.

**AC-56** `[layer: integration]`
Given an Application (Applied) custom field is marked candidate-facing and is Required,
when a candidate submits the public application form without filling that field,
then the submission is rejected with a field-level error.

**AC-57** `[layer: integration]`
Given a Job Posting custom field exists,
when a candidate visits the public application form,
then the Job Posting field is not shown to the candidate.

**AC-58** `[layer: integration]`
Given admin attempts to mark an Application — Interview field as candidate-facing,
then the option is not available for that scope.

**AC-59** `[layer: integration]`
Given admin attempts to mark an Application — Offer field as candidate-facing,
then the option is not available for that scope.

---

## Visibility

**AC-60** `[layer: e2e]`
Given a Requisition has active custom field values,
when an internal user views the Requisition detail page,
then all custom field values are displayed in an "Additional Information" section.

**AC-61** `[layer: e2e]`
Given an application has custom field values recorded across multiple stages,
when an internal user views the Candidate Profile,
then all custom field values are visible grouped by stage.

**AC-62** `[layer: integration]`
Given a field has been disabled after values were recorded,
when an internal user views a record that had a value for that field,
then the value is displayed in read-only mode with a "Campo desativado" indicator.

**AC-63** `[layer: integration]`
Given a field has been disabled,
when an internal user opens the edit form for any record,
then the disabled field is not shown in the form.

---

## Filtering

**AC-64** `[layer: integration]`
Given a ShortText custom field exists for Requisitions,
when a user filters the Requisition list by a text value that matches a stored value,
then only Requisitions with that value for that field are returned.

**AC-65** `[layer: integration]`
Given a Number custom field exists for Requisitions with a stored value of 50000,
when a user filters by a range of 40000 to 60000,
then that Requisition is included in the results.

**AC-66** `[layer: integration]`
Given a Single Choice custom field exists for Applications,
when a user filters by one of the choice values,
then only Applications with that stored choice are returned.

**AC-67** `[layer: integration]`
Given a Boolean custom field exists,
when a user filters by true,
then only records with a true value for that field are returned.

---

## Permissions

**AC-68** `[layer: integration]`
Given a logged-in user who does not have the Admin role,
when they attempt to create a custom field definition,
then the request is rejected and access is denied.

**AC-69** `[layer: integration]`
Given a logged-in user who does not have the Admin role,
when they attempt to edit a custom field definition,
then the request is rejected and access is denied.

**AC-70** `[layer: integration]`
Given a logged-in user who does not have the Admin role,
when they attempt to disable or re-enable a custom field,
then the request is rejected and access is denied.

**AC-71** `[layer: integration]`
Given a logged-in user who does not have the Admin role,
when they attempt to delete a custom field definition,
then the request is rejected and access is denied.

**AC-72** `[layer: integration]`
Given a logged-in user who does not have the Admin role but has Application edit permission,
when they fill in and submit custom fields as part of an Application update,
then the values are accepted and stored.

**AC-73** `[layer: integration]`
Given a logged-in user with view permission,
when they view an entity detail page,
then custom field values are visible in the Additional Information section.

---

## UI Rules

**AC-74** `[layer: e2e]`
Given admin opens the field builder and selects Long Text as the field type,
then the format mask input is not shown.

**AC-75** `[layer: e2e]`
Given admin opens the field builder, selects Short Text, and selects a named preset,
then the format mask input is hidden.

**AC-76** `[layer: e2e]`
Given admin opens the field builder, selects Short Text, and enters a format mask,
then the preset selector is cleared and disabled until the mask is removed.
