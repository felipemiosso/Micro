# 021: API Choice Sync — Acceptance Criteria

## API Authentication

**AC-01** `[layer: integration]`
Given the API Key is configured in the application settings,
when an external system calls the choice sync endpoint with a missing or invalid `X-API-Key` header,
then the request is rejected with a `401 Unauthorized` status.

**AC-02** `[layer: integration]`
Given the API Key is configured in the application settings,
when an external system calls the choice sync endpoint with a valid `X-API-Key` header,
then the request is authenticated and processed.

---

## Choice Synchronization

**AC-03** `[layer: integration]`
Given a `SingleChoice` custom field exists,
when the sync endpoint is called with a new list of choices,
then the custom field's active choices are updated to match the new list.

**AC-04** `[layer: integration]`
Given a `SingleChoice` custom field with active choices `["A", "B"]`,
and choice `"B"` is not saved on any record,
when the sync endpoint is called with `["A", "C"]`,
then `"B"` is completely deleted, and the active choices become `["A", "C"]`.

**AC-05** `[layer: integration]`
Given a `SingleChoice` custom field with active choices `["A", "B"]`,
and choice `"B"` is saved on an existing requisition,
when the sync endpoint is called with `["A", "C"]`,
then `"B"` is moved to `DisabledChoices` list, and the active choices become `["A", "C"]`.

**AC-06** `[layer: integration]`
Given a custom field has `"B"` in its `DisabledChoices` list,
when the sync endpoint is called with `["A", "B", "C"]`,
then `"B"` is moved back to the active `Choices` list and removed from `DisabledChoices`.

---

## Form Validation & Rendering

**AC-07** `[layer: integration]`
Given a custom field has active choices `["A", "C"]` and disabled choices `["B"]`,
when a user creates a new record and attempts to save value `"B"`,
then the validation fails because `"B"` is not an active choice.

**AC-08** `[layer: integration]`
Given a custom field has active choices `["A", "C"]` and disabled choices `["B"]`,
and a record has existing value `"B"`,
when a user updates the record without changing the custom field value,
then the validation succeeds because the existing value matches the disabled choice.


**AC-09** `[layer: e2e]`
Given a record has existing value `"B"` (which is a disabled choice),
when the edit form is loaded,
then the dropdown renders `"B"` as selected (with a visual indicator of it being retired/disabled) alongside active choices `["A", "C"]`.

---

## Manual Update Alignment


**AC-10** `[layer: integration]`
Given a custom field has active choices `["A", "B"]`,
and choice `"B"` is saved on an existing requisition,
when the admin manually updates the field definition removing `"B"` from choices (via Admin Settings UI),
then the update succeeds, and `"B"` is automatically moved to `DisabledChoices` list instead of returning a conflict error.

---

## Lookup Entity Master Data Sync

**AC-11** `[layer: integration]`
Given an external system syncs the list of departments,
when the sync request is executed,
then new departments are created, existing departments' names are updated, departments missing from the payload that have no requisitions are deleted, and departments missing from the payload that are in use are updated with `IsActive = false`.

**AC-12** `[layer: integration]`
Given an external system syncs the list of salary bands,
when the sync request is executed,
then new salary bands are created, existing salary bands' properties are updated, salary bands missing from the payload that have no requisitions are deleted, and salary bands missing from the payload that are in use are updated with `IsActive = false`.

**AC-13** `[layer: integration]`
Given an external system syncs the list of cost centers,
when the sync request is executed,
then new cost centers are created, existing cost centers' properties are updated, cost centers missing from the payload that have no requisitions are deleted, and cost centers missing from the payload that are in use are updated with `IsActive = false`.

**AC-14** `[layer: e2e]`
Given an inactive/retired department, salary band, or cost center exists in the database but is referenced by an existing Requisition,
when a user opens the edit form for that Requisition,

