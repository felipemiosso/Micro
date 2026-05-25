# 018: Custom Fields

## Overview

Admins need to capture information beyond what the system provides by default. Custom Fields allow an Admin to define additional data inputs on any supported entity — Requisition, Application (globally or per-stage), Job Posting — without requiring a developer to change code or deploy anything. Each field has a type, a label, optional help text, optional validation rules, and a display order. Field values are stored alongside the entity and shown in the relevant forms and detail views.

This feature deliberately prioritises simplicity over power. Admins are not technical:

- Validation rules are expressed in plain language ("Required", "Max 500 characters"), never as regex or code.
- For format validation (CPF, CNPJ, phone numbers), admins choose from a curated list of **named presets** grouped by tags. A field can accept **multiple presets simultaneously** — the value is valid if it matches any one of them (OR logic). This covers real-world cases like a "Documento" field that accepts either a CPF or a CNPJ.
- For custom patterns (internal codes), admins define a **format mask** using intuitive placeholder symbols (`#`, `A`, `X`) — no regex syntax.

Fields can be disabled (hidden from new forms) but their historical values are retained forever on existing records so no data is ever silently lost. Validation rules are enforced strictly: changes that would invalidate existing stored values are blocked, and every entity save validates all active custom fields in full — not just the fields the user touched.

---

## User Stories

### Field Definition (Admin)

- As an Admin, I want to create a custom field for a specific entity so that my team captures company-specific information without needing a developer.
- As an Admin, I want to choose a field type (Short Text, Long Text, Number, Date, Yes / No, Single Choice) so that the input control matches what I'm collecting.
- As an Admin, I want to mark a field as Required so that users cannot save the entity without filling it in.
- As an Admin, I want to set a character limit on text fields so that users don't paste entire documents into a one-line field.
- As an Admin, I want to set a minimum and maximum value on number fields so that obviously wrong numbers are caught immediately.
- As an Admin, I want to select one or more named format presets (e.g. CPF, CNPJ) for a text field so that the system validates the format and semantic rules without me needing to know how they work.
- As an Admin, I want presets to be organised into tag groups (e.g. "Documentos", "Contato") so that I can quickly find related presets when configuring multi-format fields.
- As an Admin, I want to define a format mask for internal codes (e.g. `DEPT-####`) so that users are guided into the correct pattern.
- As an Admin, I want to define the list of choices for a Single Choice field so that users pick from a controlled vocabulary instead of free-typing.
- As an Admin, I want to reorder custom fields so that the most important ones appear first in forms.
- As an Admin, I want to edit a field's label, help text, and validation rules after it has been created so that I can refine it over time.
- As an Admin, I want to see how many records have a value for each field so that I can make informed decisions before disabling or deleting it.
- As an Admin, I want to disable a custom field so that it no longer appears in new forms, while keeping its past values visible in historical records.
- As an Admin, I want to re-enable a disabled field so that it reappears in forms with all its prior data intact.
- As an Admin, I want to permanently delete a field that has never collected any data so that I can clean up fields created by mistake.
- As an Admin, I want to scope a custom field to a specific Application stage (Applied, Interview, Offer) or to all stages so that stage-specific fields don't pollute every step of the pipeline.
- As an Admin, I want to mark an Application custom field as candidate-facing so that the public application form collects that information directly from the candidate.

### Data Entry (Internal users with edit permission)

- As a Hiring Manager, I want to fill in the custom fields when creating or editing a Requisition so that all required company information is captured upfront.
- As a Recruiter, I want to see and fill in the custom fields relevant to the current Application stage so that I don't have to hunt for the right form.
- As a Recruiter, I want to see previously filled custom-field values in read-only format even if the field has since been disabled, so that historical context is not lost.
- As a Recruiter, I want entity list pages to be filterable by custom field values so that I can quickly find records matching specific criteria.

### Data Entry (Candidates — public form)

- As a Candidate, I want the application form to include any additional questions the employer has configured so that I can provide all the information required upfront.

### Visibility

- As a Hiring Manager, I want to see custom field values in the Requisition detail view so that I have the full picture in one place.
- As a Recruiter, I want to see custom field values on the Candidate Profile so that I can review what was captured at each stage.

---

## Supported Entities and Scopes

| Entity | Scope concept | Candidate-facing possible? |
|---|---|---|
| **Requisition** | Single scope — fields appear on the create/edit form and detail view | No |
| **Application — Global** | Shown at every stage of the pipeline | Yes |
| **Application — Applied** | Only shown while the application is in the Applied stage | Yes |
| **Application — Interview** | Only shown while the application is in the Interview stage | No |
| **Application — Offer** | Only shown while the application is in the Offer stage | No |
| **Job Posting** | Fields appear on the create/edit form and the detail view (internal only) | No |

Candidate-facing fields are limited to Application (Global) and Application (Applied) scopes because candidates submit their application at the Applied stage. Interview and Offer are internal evaluation stages that candidates have no visibility into.

> The list of supported entities is designed to grow. Adding a new target entity only requires adding a new enum value — no schema changes.

---

## Field Types

| Type | Admin label | What the user sees |
|---|---|---|
| `ShortText` | Short Text | Single-line text input |
| `LongText` | Long Text | Multi-line text area |
| `Number` | Number | Numeric input (integer or decimal) |
| `Date` | Date | Date picker (no time) |
| `Boolean` | Yes / No | Toggle or checkbox |
| `SingleChoice` | Single Choice | Dropdown with admin-defined options |

---

## Validation Rules

Admins configure validation by enabling simple rules — never by writing code or regex.

### General (all types)

| Rule | Admin sees |
|---|---|
| **Required** | "This field is required" toggle |

### Type-specific

| Rule | Applies to | Admin sees |
|---|---|---|
| **Min / Max characters** | ShortText, LongText | "Minimum characters" / "Maximum characters" numeric inputs |
| **Min / Max value** | Number | "Minimum value" / "Maximum value" numeric inputs |
| **Min / Max date** | Date | "Earliest date" / "Latest date" date pickers |
| **Choices** | SingleChoice | Add / remove / reorder choice options |

### Format validation (ShortText only — two tiers)

Some fields require a specific pattern — like a CPF or an internal product code. Because regex is too technical for most admins, format validation works in two tiers that are mutually exclusive.

#### Tier 1 — Named Presets with OR logic (developer-maintained)

Presets are organised into **tag groups** in the admin UI. An admin can select **one or more presets** for a field. A submitted value is valid if it satisfies **any one** of the selected presets. This enables patterns like a "Documento" field that accepts either a CPF or a CNPJ.

Presets ship grouped by the following tags:

**Documentos**

| Preset | What it validates |
|---|---|
| CPF | `000.000.000-00` format + Mod-11 check digit |
| CNPJ | `00.000.000/0000-00` format + Mod-11 check digit |
| PIS / PASEP | `000.00000.00-0` format |
| CNH | 11-digit driver's licence number |

**Contato**

| Preset | What it validates |
|---|---|
| E-mail | RFC 5322 compliant email address |
| Telefone (celular) | `(00) 00000-0000` |
| Telefone (fixo) | `(00) 0000-0000` |
| URL | Valid web address (http / https) |

**Localização**

| Preset | What it validates |
|---|---|
| CEP | `00000-000` eight-digit postal code |

Adding a new preset requires a developer but **no schema or migration change** — only a new entry in the validator registry on both backend and frontend. The admin dropdown is populated from a dedicated API endpoint (`GET /api/custom-fields/available-presets`) so the list is always authoritative and never falls out of sync between layers.

#### Tier 2 — Format Mask (admin-configurable)

For patterns not covered by any preset (e.g. an internal product code `DEPT-0042`), admins define a mask using simple placeholder symbols. Format masks are **not available for Long Text fields** — a masked field is by nature short and structured, which contradicts the purpose of a multi-line free-form area.

| Symbol | Matches |
|---|---|
| `#` | Any digit (0–9) |
| `A` | Any letter (a–z, A–Z) |
| `X` | Letter or digit |
| Any other character | Literal (e.g. `-`, `.`, `/` written exactly as-is) |

Example: an admin types `DEPT-####` and the system validates that the entered value matches that exact shape.

The mask also drives the **live input mask** in the UI — separators are inserted automatically as the user types, preventing most formatting errors before submission.

**Priority rule**: if any Named Preset is selected, the Format Mask input is hidden. They are mutually exclusive.

There is intentionally no raw regex input, no cross-field dependencies, and no conditional logic in this spec.

---

## Validation Strictness Rules

These rules govern how the system behaves when validation constraints change or when records are saved.

### Rule changes that invalidate existing stored values are blocked

Before saving any change to a field's validation configuration, the backend checks whether any existing stored value would be invalidated by the change. If at least one value would be invalidated, the change is **rejected** and the admin is shown how many records are affected.

Examples of blocked changes:
- Decreasing the max character length when existing values exceed the new limit.
- Adding a new max character limit for the first time when existing values already exceed it.
- Removing a Single Choice option that is stored in any existing record.

The admin must resolve the conflict in the data before the rule change can be saved, or disable the field and create a new one.

### Every entity save validates all active custom fields in full

When a user saves an entity (create or update), the backend validates **all active custom fields** for that entity — including fields whose values were not changed in the current form submission. If any active field value violates the current rules, the entire save is rejected with field-level error messages.

This means: if an admin successfully adds a new constraint (e.g. marks a field as Required) and existing records have no value for that field, those records cannot be saved again until the field is filled. The admin is shown a count of affected records when making the Required change, so they can make an informed decision.

---

## Admin Management Page

The Custom Fields management page is organised per entity. The admin selects an entity (Requisition, Application, Job Posting) and sees two sections.

### Active Fields

- A draggable, ordered list of all active fields for the selected entity / scope.
- Each row shows: label, type, scope (for Application), whether Required, and a **record count badge** (e.g. "47 records") indicating how many entities have a value stored for that field.
- The record count is loaded with the list — not on demand — so the admin can see it before making any decision.
- Actions per row: **Edit**, **Disable**, **Delete**.
  - The **Delete** button shows a tooltip on hover when records exist: *"Has 47 recorded values — can only be disabled."*

### Disabled Fields

- A collapsed section showing all disabled fields.
- Each row shows: label, type, record count, and date disabled.
- Actions: **Re-enable**, **Delete** (only enabled if record count is 0).

---

## Creation Flow

1. Admin opens Settings → Custom Fields, selects an entity.
2. Clicks **Add Field**.
3. A slide-over panel opens with the field builder:
   - Label (required)
   - Help text (optional placeholder or instruction shown below the input)
   - Field type selector
   - Validation options that update dynamically based on the chosen type
   - Required toggle
   - For Application scopes (Global and Applied only): **"Ask candidate on application form"** toggle
4. Admin clicks **Save Field**.
5. The system validates the definition itself (label present, type valid, mask syntax valid, choices non-empty for Single Choice, preset selections consistent).
6. On success, the new field is appended at the bottom of the Active Fields list.
7. **The field is immediately live** — no deployment, no restart. The next time any user opens a create or edit form for that entity, the field appears.

When a new field is added to an entity, existing records are affected as follows:
- On **edit forms**: the field appears empty. If marked Required, the record cannot be saved without filling it in.
- On **detail views**: the field is not shown until a value exists for that record.

---

## Candidate-Facing Fields

Application custom fields with the **"Ask candidate on application form"** toggle enabled appear in the public job application form alongside the standard candidate fields (name, email, phone, resume).

- Only fields scoped to **Application — Global** or **Application — Applied** can be candidate-facing.
- The public application form loads candidate-facing fields per job posting at render time.
- Candidate-submitted values are stored as regular custom field values on the Application record.
- Internal users see these values in the same "Additional Information" section as any other custom field value.
- Candidate-facing fields are subject to the same validation rules (Required, format, presets) on submission.

---

## Filling in Custom Fields (End-User Flow)

Custom fields appear as a dedicated **"Additional Information"** section at the bottom of the relevant create / edit form, after all standard fields. They are rendered in the configured order.

- The form loads custom field definitions in **parallel** with the main entity data — there is no sequential wait and no separate loading state.
- Frontend validation (required, min / max, preset OR logic, mask) runs in real time as the user types.
- On submit, the backend independently re-validates **all active custom field values** against the stored definitions. Frontend validation is a UX convenience — the backend is always the source of truth and cannot be bypassed.
- If any value fails backend validation (including values not touched in the current session), field-level error messages are shown inline next to the relevant field.

On detail and profile views, custom field values are shown in a read-only **"Additional Information"** section. Values from disabled fields are included with a visual indicator: *"Campo desativado"*.

### Filtering

On entity list pages (Requisitions, Applications), users can filter records by custom field values. The filter UI dynamically renders the appropriate input control based on the field type (text search, number range, date range, choice picker, boolean toggle).

---

## Editing an Existing Field

Admins can edit a field's definition at any time. The set of allowed changes depends on whether the field already has recorded values.

| Change | Allowed when values exist? |
|---|---|
| Label | ✅ Always |
| Help text | ✅ Always |
| Required → optional | ✅ Always |
| Optional → Required | ✅ Allowed — admin is shown count of existing records with blank values; those records will be blocked on next save until the field is filled |
| Max length (increase) | ✅ Safe — no existing value is invalidated |
| Max length (decrease or new limit added) | ❌ Blocked if any existing value exceeds the new limit; admin is shown affected record count |
| Add a Single Choice option | ✅ Always |
| Remove a Single Choice option | ❌ Blocked if that option is stored in any existing record |
| Change field type | ❌ Blocked if any value exists |
| Change / add named preset | ❌ Blocked if any value exists |
| Change format mask | ❌ Blocked if any value exists |

When a blocked change is attempted, the system explains why in plain language and suggests the alternative: disable the field and create a new one.

---

## Disable / Re-enable Flow

**Disabling a field:**

1. Admin clicks Disable (or is redirected here when attempting to delete a field with values).
2. A confirmation dialog explains what will happen: the field disappears from forms; past values are preserved.
3. On confirm, the field is immediately removed from all create and edit forms.
4. Existing values remain visible in detail and profile views with a *"Campo desativado"* label.
5. The field moves to the Disabled Fields section in the management page.

**Re-enabling a field:**

1. Admin clicks Re-enable in the Disabled Fields section.
2. The field immediately reappears in all relevant create and edit forms.
3. All previously recorded values are visible again — they were never removed.
4. The field's record count and order are preserved from before it was disabled.

---

## Deletion Flow

**Field has zero recorded values (never used):**

1. Admin clicks Delete.
2. Confirmation dialog: *"Excluir este campo permanentemente? Esta ação não pode ser desfeita."*
3. On confirm, the field definition is hard-deleted from the database.

**Field has one or more recorded values:**

1. Admin clicks Delete.
2. The system does **not** offer a delete confirmation. Instead it shows an information dialog:
   > *"Este campo possui dados em N registros. Excluí-lo permanentemente tornaria esses dados inacessíveis para sempre. Você pode desativá-lo — ele deixará de aparecer em novos formulários, mas todos os dados históricos continuarão visíveis nos registros existentes."*
3. Two options: **Desativar campo** or **Cancelar**. Hard delete is not offered.
4. The backend enforces this restriction at the API level — a direct API call attempting to hard-delete a field with values is rejected with a structured error (`FIELD_HAS_VALUES`).

---

## Ordering

- Each field has an `Order` integer within its entity / scope grouping.
- Admins can drag-and-drop fields to reorder them in the management page.
- Order determines the sequence in forms, detail views, and the management list.
- Newly created fields are appended to the end (highest order number in the group).
