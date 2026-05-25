# Log — 2026-05-24

**User:** now I need you to write a spec that allows admins to add custom fields to our entities, like requisition, application (every stage of it) and all others. Admins should be able to set validations to fields, we need to think a easy way cause admins are not tech savvy. How do you think we can do that and what are the implications of that in terms of changing the code, what whould happens when deletting a field for entities already created and so on?

**Gemini:** Let me explore the codebase first to understand the existing structure before writing the spec.

[explored product specs, EF entities, models including Requisition.cs, Application.cs, JobPosting.cs, InterviewDetails.cs, OfferDetails.cs, Feedback.cs]

The [business.md](file:///C:/Users/felip/source/Micro/product/018-custom-fields/business.md) is written and the technical analysis artifact above captures everything you need to make decisions. Here's a summary of what I've proposed and what needs your call:

## What the spec proposes

**Two new DB tables, zero changes to existing entity tables:**
- `CustomFieldDefinition` — the field's label, type, validation rules, scope, and order
- `CustomFieldValue` — a flat `EntityId → string value` store for every filled-in answer

All values are stored as strings and parsed by the app layer based on the known type. This keeps the schema simple and completely decoupled from the existing entities.

**6 field types** with plain-language validation (no regex, ever):
- Short Text / Long Text (character limits) → Number (min/max) → Date (date range) → Yes/No → Single Choice (admin-defined options)

**Scoping** lets an admin say "this field only appears during the Interview stage" vs. "this field appears on every Application stage".

## The 5 design decisions you need to make

These are the open questions in the spec — each has a recommendation but they're yours to confirm:

| # | Question | Recommendation |
|---|---|---|
| 1 | Can admins change a field's **type** once data exists? | ❌ Block it — disable + recreate |
| 2 | **Required fields** on pre-existing records — validate retroactively? | Only on explicit re-save |
| 3 | **Hard-delete** a field that has recorded values? | Block — offer Disable only |
| 4 | Removing a **Single Choice option** that is already stored somewhere | Needs your pick: strict (block) vs. permissive (keep stored value, hide from new picks) |
| 5 | Gate the feature behind a `CustomField:Manage` **permission**, or Admin-only? | Add the permission for future flexibility |

Once you confirm these I can write the `acceptance.md`, `backend.md`, and `frontend.md` to complete the spec.

---

**User:** what about fields validation, for example, in Brazil we have CPF that must follow a pattern. How can we do that so users no familiar with tech can add custom validations?

**Gemini:** Great question — this exposes a real gap in the spec. CPF is actually a perfect stress test because it has **two layers of validation** that need to work together:

1. **Format** — `000.000.000-00` (three digits, dot, three, dot, three, dash, two)
2. **Semantics** — the last two digits are a **check digit** calculated from the first nine. A regex alone can't catch `123.456.789-00` being invalid even though it matches the format.

This means "let admins type a regex" doesn't even fully solve CPF — and regex is already too technical. So here's the design I'd propose:

## Two-Tier Validation System

### Tier 1 — Named Presets (developer-maintained, curated)
A dropdown of built-in validators with friendly names. These live in code and can include full semantic logic:

| Admin sees | What it validates |
|---|---|
| CPF | Format + check digit algorithm |
| CNPJ | Format + check digit algorithm |
| E-mail | RFC-compliant email format |
| Phone (Brazil) | Mobile and landline formats |
| CEP | 8-digit postal code |
| URL | Valid web address |
| *(none)* | No format restriction |

The admin picks from a list. Zero code knowledge required. Developers add new presets when the business needs them.

### Tier 2 — Format Mask (user-configurable)
For simple patterns that don't need semantic logic (e.g. a product code like `DEPT-0000`), admins define a mask using familiar placeholder symbols — not regex:

| Symbol | Means |
|---|---|
| `#` | Any digit (0–9) |
| `A` | Any letter |
| `X` | Letter or digit |
| Everything else | Literal character |

So an admin types `DEPT-####` and the field validates `DEPT-0042`. No regex, no tech knowledge.

The key rule: **Tier 1 takes priority**. If a named preset is selected, the mask is ignored. The mask is only available when no preset is chosen.

## What this means for `ValidationJson`

```json
// CPF — preset handles everything
{ "validationPreset": "cpf" }

// Product code — mask-based
{ "formatMask": "DEPT-####" }

// Number with limits (unchanged)
{ "min": 0, "max": 9999 }

// Text with a mask + max length
{ "formatMask": "XX-####", "maxLength": 10 }
```

## Implications

**For developers**: A new `ValidatorRegistry` (or equivalent static class) maps preset names to validation functions in C#. Adding CPF requires implementing the check digit algorithm once. Adding a new preset later is a one-line registration — no schema change, no migration. On the frontend, the same preset key drives the Angular form validator.

**For the feature scope**: The initial preset library should be decided before building. For a Brazilian-market ATS I'd recommend shipping: **CPF, CNPJ, Email, Phone (Brazil), CEP** in v1.

**For format masks**: The mask is also used as an **input mask** in the UI (the field auto-inserts dots and dashes as you type), not just a post-validation rule. This dramatically reduces user errors.

[business.md updated with two-tier validation system]

---

**User:** how would I make dynamic validations work in both frontend and backend? Lets say admin is adming a new field with a specific rule, how do you see it integrating in the system?

**Gemini:** This is the most interesting part of the feature, because the challenge is that validation logic that normally lives in code has to become **data-driven at runtime**. Let me walk through the full integration end-to-end.

## The Core Insight

The frontend and backend cannot hardcode field rules — they need to read them from the database definition at runtime and translate them into their respective validation mechanisms. But the **backend is always the source of truth**; frontend validation is purely a UX convenience.

```
Admin defines field
       │
       ▼
CustomFieldDefinition row (DB)
       │
       ├──► Angular reads definition → builds FormGroup with dynamic ValidatorFns
       │           (client-side: fast feedback, no round-trip)
       │
       └──► .NET reads definition → runs CustomFieldValidator on submit
                    (server-side: always enforced, cannot be bypassed)
```

## Step 1 — The Admin saves a field definition

`POST /api/custom-fields`

```json
{
  "targetEntity": "Requisition",
  "fieldType": "ShortText",
  "label": "CPF do Responsável",
  "helpText": "CPF da pessoa que aprovou a vaga",
  "isRequired": true,
  "validation": {
    "preset": "cpf"
  }
}
```

The API stores one `CustomFieldDefinition` row. `ValidationJson` is serialised from the `validation` object.

## Step 2 — Angular builds a live form dynamically

When the Requisition create/edit page loads, it calls `GET /api/custom-fields?entity=Requisition&activeOnly=true`. Angular uses a `CustomFieldFormService` to translate each definition into a reactive `FormControl` with the right `ValidatorFn[]`.

**Named presets** are Angular validator functions registered in a lookup. **Format masks** drive both validation and the input mask (auto-inserts separators as you type).

## Step 3 — User submits the form

Angular sends the custom field values alongside the main payload:

```json
POST /api/requisitions
{
  "title": "Analista de Dados",
  "departmentId": "...",
  "customFieldValues": [
    { "definitionId": "a1b2-...", "value": "123.456.789-09" },
    { "definitionId": "c3d4-...", "value": "42000" }
  ]
}
```

## Step 4 — Backend re-validates everything

The .NET API never trusts the frontend. It loads all active definitions for the entity, then runs a `CustomFieldValidator` against each submitted value. The `ValidatorRegistry` in C# mirrors the frontend's `presetRegistry`.

## How presets stay in sync between frontend and backend

`GET /api/custom-fields/available-presets` returns the canonical list of preset keys with labels. The Angular admin UI fetches this to populate the dropdown — it never hardcodes which presets exist. If a preset key exists in a stored definition but the frontend doesn't know it yet, the frontend simply skips client-side validation for that field — the backend will still enforce it.

## Summary of the full round-trip

```
Admin saves definition (preset: "cpf")
        │
        ▼
DB: CustomFieldDefinition { ValidationJson: '{"preset":"cpf"}' }
        │
        ├──► Angular: GET /api/custom-fields?entity=Requisition
        │    → buildFormGroup() → FormControl with presetValidator("cpf")
        │    → real-time feedback as user types
        │
        └──► User submits → POST /api/requisitions { customFieldValues: [...] }
                    │
                    ▼
             .NET loads definitions from DB
             CustomFieldValidator.Validate(def, value)
             ValidatorRegistry.Validate("cpf", "123.456.789-09")
             → CpfValidator.Validate() runs Mod-11 algorithm
             → valid → saves CustomFieldValue row
             → invalid → 422 ValidationProblem → Angular shows server error
```

---

**User:** and how do you see the flow of creating and adding the custom field into one of the entities? the same goes for deletion

**Gemini:** Good question — this is where the UX decisions really matter. Let me walk through both flows in full, including the nuances that make or break the experience.

## Creation Flow

### Admin side (field definition)

[mermaid sequence diagram showing Admin → Angular Admin UI → .NET API → PostgreSQL flow for creating a field definition, ending with field immediately live]

The field is **live immediately** — no deployment, no restart. The next time any user opens a create or edit form for that entity, the field appears.

### End-user side (filling in the field)

[mermaid sequence diagram showing Hiring Manager → Requisition Form → .NET API → PostgreSQL flow, including parallel load of definitions and main entity data, real-time validation, and backend re-validation on submit]

**One important detail**: the form loads definitions and the main entity data in **parallel** (`forkJoin`), not sequentially, so there's no extra loading state for custom fields.

## Deletion Flow

[mermaid flowchart showing: Admin clicks Delete → valueCount > 0? → No: confirm hard delete → Yes: info dialog offering only Disable option]

### The key UX trick — don't surprise the admin at delete time

The `valueCount` is **included in the definition list response** from the start. The UI uses this to show a badge next to each field — `47 records` — so the admin already knows before clicking delete. The delete button can also show a tooltip on hover: *"Has 47 recorded values — can only be disabled."*

### Backend enforcement (cannot be bypassed via API)

Even if someone calls the API directly (bypassing the UI), the backend blocks the hard delete and returns 409 Conflict with `FIELD_HAS_VALUES` code.

## Disable / Re-enable Flow

[mermaid sequence diagram showing Admin disabling field → field disappears from forms, values remain in detail views with "Field no longer in use" label → Admin re-enables → field reappears in forms with all prior data restored]

## What "disabled but visible" looks like in a record

```
Additional Information
─────────────────────
CPF do Responsável        123.456.789-09    ⚠ Field no longer in use
Budget Code               DEPT-0042
```

## Edit flow (quick summary)

[table showing allowed/blocked changes based on whether values exist: label/help text always OK; type change/preset change blocked if values exist; max length decrease shows warning; required→optional always OK; add SingleChoice option always OK; remove SingleChoice option blocked if stored]

---

**User:** I want you to enrich spec with what we just talked and later enumerate the open questions

**Gemini:** [Rewrote business.md with full creation flow, end-user form flow, admin management page behaviour, two-tier format validation, editing constraints, disable/re-enable and deletion decision trees]

The [business.md](file:///C:/Users/felip/source/Micro/product/018-custom-fields/business.md) is fully rewritten. Here's what was added compared to what we had before:

**New sections:** Admin Management Page, Creation Flow, Filling in Custom Fields (End-User Flow), Editing an Existing Field, Disable / Re-enable Flow, Deletion Flow.

**8 open questions**, consolidated and numbered clearly:

| # | Topic |
|---|---|
| 1 | Which presets ship in v1? |
| 2 | Single Choice option removal — strict or permissive? |
| 3 | Filtering/search on custom fields — in or out of scope? |
| 4 | `CustomField:Manage` permission or Admin-only? |
| 5 | Candidate-facing fields on public form — in scope? |
| 6 | Format Mask on LongText? |
| 7 | Decreasing max length when values exist — block or warn? |
| 8 | Retroactive validation on records with over-limit values |

Questions **1, 2, and 4** are the ones with the highest impact on the implementation — good to settle those before writing `acceptance.md`.

---

**User:** 1. the most common from Brazil, also presets should have a tag system so we can join them.
2. what do you mean?
3. in
4. admin-only
5. yes
6. what do you mean?
7. examples?
8. examples?

**Gemini:** Let me address each one clearly — some need examples for you to decide, others I have clarifying questions on.

## ✅ Resolved — updating spec now

**3 (Filtering — in scope):** Noted. Custom field values will be filterable on entity lists.
**4 (Admin-only):** No `CustomField:Manage` permission — only the Admin role can manage definitions.

## 🇧🇷 #1 — Brazil presets + "tag system to join them"

Proposed initial preset list for Brazil: [CPF, CNPJ, PIS/PASEP, CNH, E-mail, Telefone celular, Telefone fixo, CEP, URL]

I need to clarify **"tag system to join them"**:

> **Option A — OR logic (multi-select):** A single field can accept *any* of multiple presets. For example, a "Documento Fiscal" field accepts either CPF *or* CNPJ — the value is valid if it matches either one. Tags group presets so the admin can quickly pick related ones.

> **Option B — Organisation only:** Tags are just for filtering the preset dropdown in the admin UI. A field can still only have one preset.

## ❓ #2 — Single Choice option removal, explained with an example

Say an admin creates a "Nível de Senioridade" field with options: Júnior, Pleno, Sênior, Lead. 50 Applications already have Sênior stored. Admin wants to remove Sênior.

**Strict (block removal):** System says cannot remove — it is stored in 50 records. Rename it instead or disable the field.

**Permissive (allow removal, keep stored value):** Removes Sênior from dropdown for future picks, but 50 records still display Sênior with a *(opção removida)* label.

## ❓ #5 — Candidate-facing fields (yes, but needs a design decision)

**Option A — Job Posting:** Field defined on Job Posting. Value collected during application and stored against the Application record.

**Option B — Application (Applied stage) with a flag:** Field defined on Application entity at Applied scope with `IsCandidateFacing = true`. Appears in public form and internal Applied-stage view. Recommend Option B.

## ❓ #6 — Format Mask on LongText, explained

A format mask like `DEPT-####` forces exact shape and length. LongText is a multi-line text area for free-form content. They contradict each other. Should the Format Mask option appear in the UI when admin picks "Long Text"? My recommendation: no.

## ❓ #7 — Max length decrease, concrete example

Admin created "Código de Projeto" (ShortText, max 100 chars). Record has 42-char value stored. Admin changes max to 20.

- **Block the change:** System says cannot reduce max — 3 records already have longer values.
- **Allow with warning:** Change saves. 42-char value continues to display as-is. If that record is opened for editing, the custom field fails validation.
- **Allow silently:** Change saves. Existing values untouched.

## ❓ #8 — Retroactive validation, concrete example

Max is now 20 chars but a Requisition still has a 42-char value stored. Recruiter opens that Requisition, changes only the Title field, hits Save.

- **Yes, reject the save:** Backend validates all custom fields on any save, including the untouched 42-char one.
- **No, only validate what was touched:** Backend only validates values explicitly submitted. The 42-char value stays and the recruiter's unrelated save goes through.

---

**User:** tags: option a
#2: strict
#5: the candidate can only fill in application fields, but job posting is also an entity that is allowed by the admin to have custom fields
#6: no
#7: block the change
#8: reject the save.

**Gemini:** All 8 questions are now resolved. [Rewrote business.md with all decisions incorporated: OR-logic preset tags, Brazil preset library, strict Single Choice removal, filtering in scope, candidate-facing Application fields, format mask excluded from LongText, blocked max-length decreases, full-field validation on every save. Open questions section removed entirely.]

business.md done — all 8 open questions resolved and closed, no open questions section remaining.

| Decision | What was locked in |
|---|---|
| **Presets** | OR logic — field accepts value matching any selected preset; organised by tags (Documentos, Contato, Localização) |
| **Brazil preset library** | CPF, CNPJ, PIS/PASEP, CNH, Email, Celular, Fixo, URL, CEP |
| **Single Choice removal** | Strict — blocked if the option is stored anywhere |
| **Filtering** | In scope — entity list pages filter by custom field values |
| **Permissions** | Admin-only — no separate permission |
| **Candidate-facing** | Application (Global + Applied) fields only; values stored on the Application record |
| **Format Mask on LongText** | Not available |
| **Max length decrease** | Blocked if any existing value would exceed the new limit |
| **Retroactive validation** | Every save validates all active fields in full |

---

**User:** /caveman:caveman full

**Gemini:** Caveman mode: full. Activated.

`business.md` done. Zero open questions. All decisions locked.

Next: `acceptance.md`. Want me to write it?

---

**User:** greate, dont forget to log our chat. do it

**Gemini:** [logged conversation to product/history/2026-05-24/log.md]

---

**User:** yes

**Gemini:** [`acceptance.md`](file:///C:/Users/felip/source/Micro/product/018-custom-fields/acceptance.md) done. 76 scenarios.

Coverage map:

| Area | ACs |
|---|---|
| Create field (all types, presets, mask, OR-logic) | AC-01 → AC-14 |
| Ordering | AC-15 → AC-16 |
| Edit — safe changes | AC-17 → AC-21 |
| Edit — blocked changes (type, preset, mask, length, choices) | AC-22 → AC-29 |
| Making field Required | AC-30 → AC-31 |
| Disable / re-enable | AC-32 → AC-35 |
| Deletion (zero values vs has values) | AC-36 → AC-38 |
| End-user validation (required, text, number, date, choice) | AC-39 → AC-48 |
| Retroactive validation (untouched fields) | AC-49 → AC-50 |
| Stage scoping | AC-51 → AC-54 |
| Candidate-facing fields | AC-55 → AC-59 |
| Visibility (detail views, disabled label) | AC-60 → AC-63 |
| Filtering | AC-64 → AC-67 |
| Permissions | AC-68 → AC-73 |
| UI rules (mask hidden for LongText, preset/mask mutex) | AC-74 → AC-76 |

`backend.md` next?

---

**User:** yes

**Gemini:** [`backend.md`](file:///C:/Users/felip/source/Micro/product/018-custom-fields/backend.md) done. Key decisions captured:

**New infrastructure:**
- `ValidatorRegistry` — static dictionary of preset keys → `(Label, Tag, Validate fn)`. Single source of truth for both the `/available-presets` endpoint and runtime validation
- `CustomFieldValidator` — pure static function, type-switches on `FieldType`, applies OR logic across multiple presets
- `MaskValidator` — pure static, `#/A/X/literal` matching
- `CustomFieldPersistence` — three methods (`ValidateAsync`, `PersistAsync`, `GetValuesAsync`) that every entity endpoint calls; keeps handlers lean

**Key architectural choices:**
- `ValidateAsync` merges **submitted + stored** values and validates all active fields — this is how retroactive validation is enforced without needing extra logic in each endpoint
- `DeleteBehavior.Restrict` on the FK from `CustomFieldValue → CustomFieldDefinition` — EF enforces the no-cascade rule at DB level
- `ValidationJson` stored as `jsonb` (PostgreSQL) — allows future indexed queries if needed
- Application stage scoping uses enum value matching; the handler decides which scopes to load based on current `ApplicationStatus`
- Filtering uses `EXISTS` subqueries against the values table, driven by the composite index on `(EntityId, TargetEntity)`

`frontend.md` next?

---

**User:** yes

**Gemini:** [`frontend.md`](file:///C:/Users/felip/source/Micro/product/018-custom-fields/frontend.md) done. Spec 018 complete — all 4 files written.

Quick map of what the frontend spec covers:

| Area | Key decision |
|---|---|
| **`CustomFieldFormService`** | Single service builds FormGroup, applies server errors, extracts values — keeps all form logic off components |
| **`custom-field.validators.ts`** | OR logic via `presetValidator(string[])` — mirrors backend's `ValidatorRegistry.ValidateAny`; unknown preset keys silently skip (backend enforces) |
| **`InputMaskDirective`** | Custom lightweight directive (`#/A/X/literal`), no npm dep; preset display masks derived from `PRESET_DISPLAY_MASKS` constant |
| **`PresetSelectorComponent`** | Grouped checkboxes from `/available-presets` — no hardcoded preset names in Angular |
| **Preset ↔ Mask exclusivity** | `effect()` disables the mask control whenever any preset is selected |
| **Edit-mode locked fields** | `fieldType`, `presets`, `formatMask` disabled in form when `valueCount > 0`; notice rendered below |
| **Delete flow** | `valueCount === 0` → confirm hard delete via `ConfirmDialogComponent`; `> 0` → info dialog offers only Disable |
| **Stage-aware scopes** | `visibleScopes` computed signal derives which `CustomFieldTargetEntity` values to show based on `ApplicationStatus` |
| **Routing** | New `/admin/custom-fields` route guarded by `roleGuard('Admin')`; filter state reflected in URL via `queryParams` |

---

**User:** are we ready to implement?

**Gemini:** Let me do a quick readiness check before saying yes.

Got everything I need. Clear answer:

---

**Almost. Two blockers, one note.**

### 🔴 Blocker 1 — EF Migrations don't exist yet

Project uses `MigrateAsync()` at startup — this requires actual migration files. No `Migrations/` folder exists. Before implementing, need to:

```powershell
dotnet ef migrations add InitialCreate --project src/Micro.API
```

...to snapshot the current schema, **then** add the custom fields migration on top. Skipping this means `MigrateAsync()` crashes at startup with no baseline to migrate from.

### 🔴 Blocker 2 — `ValidationJson` stored as `jsonb` needs Npgsql config

The EF config uses `.HasColumnType("jsonb")`. PostgreSQL `jsonb` with EF on Npgsql requires enabling the `LegacyTimestampBehavior` or ensuring `EnableDynamicJson` is off — needs a quick check during implementation that serialization works as expected with the `ValidationOptions` record.

### 🟡 Note — no separate `ngx-mask`, using custom directive

`@angular/cdk` ✅ already installed (v21.2.11). Angular 21 ✅ — all signal APIs (`model()`, `input()`, `effect()`) available. The custom `InputMaskDirective` in the spec is the right call — no extra dep needed.

---

**Recommended sequence before writing any code:**

1. Generate baseline EF migration (`InitialCreate`) first
2. Then add custom fields migration on top
3. Then backend, then frontend

Want me to start with the migration baseline?

---

**User:** migrations folder is in the wrong locations, it is in the root of Micro.API instead of Data. Move it, please.

**Gemini:** I've moved the Migrations folder to `src/Micro.API/Data/Migrations/` and updated the namespaces in all moved files to `Micro.API.Data.Migrations`. I also ran a successful build of the solution to confirm everything compiles clean.

