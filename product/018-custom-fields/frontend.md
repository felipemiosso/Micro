# 018: Custom Fields — Frontend Design

## File Map

### New Files

```
src/Micro.Web/src/app/
  core/
    services/
      custom-fields.service.ts
      custom-field-form.service.ts
    ui/
      custom-field-input/
        custom-field-input.ts
      custom-field-display/
        custom-field-display.ts
      custom-field-filter/
        custom-field-filter.ts
      input-mask.directive.ts
    validators/
      custom-field.validators.ts
  features/
    admin/
      custom-fields/
        custom-fields-admin.ts
        definition-list/
          definition-list.ts
          definition-row.ts
        definition-builder/
          definition-builder.ts
          preset-selector.ts
```

### Modified Files

```
src/Micro.Web/src/app/
  app.routes.ts                                        ← add custom-fields child route
  features/
    admin/
      admin-settings.ts                                ← add Custom Fields nav entry
    requisitions/
      form/requisition-form.ts                         ← inject custom fields section
      list/requisition-list.ts                         ← inject custom field filters
    applications/
      apply/apply.ts                                   ← inject candidate-facing fields
      detail/application-detail.ts                     ← inject custom field display
      applications-board/applications-board.ts         ← inject stage custom field section
    job-postings/
      admin-edit/job-posting-edit.ts                   ← inject custom fields section
```

---

## Models / Contracts

Place in `core/services/custom-fields.service.ts` alongside the service:

```typescript
export type CustomFieldTargetEntity =
  | 'Requisition'
  | 'Application_Global'
  | 'Application_Applied'
  | 'Application_Interview'
  | 'Application_Offer'
  | 'JobPosting';

export type CustomFieldType =
  | 'ShortText' | 'LongText' | 'Number' | 'Date' | 'Boolean' | 'SingleChoice';

export interface ValidationOptions {
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  minDate?: string;       // ISO date yyyy-MM-dd
  maxDate?: string;
  presets?: string[];     // OR logic — keys from available-presets
  formatMask?: string;
  choices?: string[];
}

export interface CustomFieldDefinition {
  id: string;
  targetEntity: CustomFieldTargetEntity;
  fieldType: CustomFieldType;
  label: string;
  helpText?: string;
  order: number;
  isRequired: boolean;
  isDisabled: boolean;
  isCandidateFacing: boolean;
  validation?: ValidationOptions;
  valueCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CustomFieldValueDto {
  definitionId: string;
  label: string;
  fieldType: CustomFieldType;
  value?: string;
  isDisabled: boolean;
}

export interface PresetGroup {
  tag: string;
  presets: PresetItem[];
}

export interface PresetItem {
  key: string;
  label: string;
}

export interface CustomFieldValueInput {
  definitionId: string;
  value: string | null;
}
```

---

## Services

### `custom-fields.service.ts`

```typescript
@Injectable({ providedIn: 'root' })
export class CustomFieldsService {
  private http = inject(HttpClient);

  getDefinitions(
    entity: CustomFieldTargetEntity,
    options: { includeDisabled?: boolean } = {}
  ): Observable<CustomFieldDefinition[]> {
    const params = new HttpParams()
      .set('entity', entity)
      .set('includeDisabled', options.includeDisabled ?? false);
    return this.http.get<CustomFieldDefinition[]>('/api/custom-fields', { params });
  }

  getAvailablePresets(): Observable<PresetGroup[]> {
    return this.http.get<PresetGroup[]>('/api/custom-fields/available-presets');
  }

  createDefinition(req: CreateCustomFieldRequest): Observable<CustomFieldDefinition> {
    return this.http.post<CustomFieldDefinition>('/api/custom-fields', req);
  }

  updateDefinition(id: string, req: UpdateCustomFieldRequest): Observable<CustomFieldDefinition> {
    return this.http.put<CustomFieldDefinition>(`/api/custom-fields/${id}`, req);
  }

  disableDefinition(id: string): Observable<void> {
    return this.http.patch<void>(`/api/custom-fields/${id}/disable`, {});
  }

  enableDefinition(id: string): Observable<void> {
    return this.http.patch<void>(`/api/custom-fields/${id}/enable`, {});
  }

  reorderDefinitions(orderedIds: string[]): Observable<void> {
    return this.http.put<void>('/api/custom-fields/reorder', { orderedIds });
  }

  deleteDefinition(id: string): Observable<void> {
    return this.http.delete<void>(`/api/custom-fields/${id}`);
  }
}
```

### `custom-field-form.service.ts`

Bridges field definitions to Angular reactive forms. Injected by any component that needs to render custom field inputs.

```typescript
@Injectable({ providedIn: 'root' })
export class CustomFieldFormService {

  buildFormGroup(
    definitions: CustomFieldDefinition[],
    existingValues: CustomFieldValueDto[] = []
  ): FormGroup {
    const controls: Record<string, FormControl> = {};
    const valueMap = Object.fromEntries(existingValues.map(v => [v.definitionId, v.value ?? '']));

    for (const def of definitions) {
      controls[def.id] = new FormControl(
        valueMap[def.id] ?? '',
        this.buildValidators(def)
      );
    }
    return new FormGroup(controls);
  }

  buildValidators(def: CustomFieldDefinition): ValidatorFn[] {
    return buildCustomFieldValidators(def); // delegates to custom-field.validators.ts
  }

  /** Patches server-side validation errors from ValidationProblem onto form controls. */
  applyServerErrors(
    errors: Record<string, string[]>,
    customFieldsGroup: FormGroup
  ): void {
    for (const [key, messages] of Object.entries(errors)) {
      if (key.startsWith('customFields.')) {
        const defId = key.replace('customFields.', '');
        customFieldsGroup.get(defId)?.setErrors({ serverError: messages[0] });
      }
    }
  }

  /** Extracts CustomFieldValueInput[] from the form group for submission. */
  extractValues(
    group: FormGroup,
    definitions: CustomFieldDefinition[]
  ): CustomFieldValueInput[] {
    return definitions.map(def => ({
      definitionId: def.id,
      value: group.get(def.id)?.value ?? null
    }));
  }
}
```

---

## Validators — `custom-field.validators.ts`

```typescript
// Frontend preset registry — mirrors ValidatorRegistry.cs on the backend.
// Keys must match exactly. Unknown keys are skipped (backend enforces them).
const PRESET_REGISTRY: Record<string, (value: string) => boolean> = {
  cpf:            validateCpf,
  cnpj:           validateCnpj,
  pis:            validatePis,
  cnh:            validateCnh,
  email:          (v) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v),
  phone_mobile:   (v) => /^\(\d{2}\) \d{5}-\d{4}$/.test(v),
  phone_landline: (v) => /^\(\d{2}\) \d{4}-\d{4}$/.test(v),
  url:            (v) => { try { new URL(v); return true; } catch { return false; } },
  cep:            (v) => /^\d{5}-\d{3}$/.test(v),
};

// OR logic: passes if value satisfies at least one preset
export function presetValidator(presets: string[]): ValidatorFn {
  return (control) => {
    if (!control.value) return null;
    const known = presets.filter(k => PRESET_REGISTRY[k]);
    if (known.length === 0) return null;
    return known.some(k => PRESET_REGISTRY[k](control.value)) ? null : { invalidPreset: true };
  };
}

export function maskValidator(mask: string): ValidatorFn {
  return (control) => {
    if (!control.value) return null;
    return matchesMask(mask, control.value) ? null : { invalidMask: { mask } };
  };
}

function matchesMask(mask: string, value: string): boolean {
  if (mask.length !== value.length) return false;
  return [...mask].every((token, i) => {
    const c = value[i];
    if (token === '#') return /\d/.test(c);
    if (token === 'A') return /[a-zA-Z]/.test(c);
    if (token === 'X') return /[a-zA-Z0-9]/.test(c);
    return c === token;
  });
}

function minDateValidator(minDate: string): ValidatorFn {
  return (control) => {
    if (!control.value) return null;
    return control.value >= minDate ? null : { minDate: { minDate } };
  };
}

function maxDateValidator(maxDate: string): ValidatorFn {
  return (control) => {
    if (!control.value) return null;
    return control.value <= maxDate ? null : { maxDate: { maxDate } };
  };
}

export function buildCustomFieldValidators(def: CustomFieldDefinition): ValidatorFn[] {
  const v: ValidatorFn[] = [];
  if (def.isRequired) v.push(Validators.required);

  const opts = def.validation;
  if (!opts) return v;

  switch (def.fieldType) {
    case 'ShortText':
      if (opts.minLength) v.push(Validators.minLength(opts.minLength));
      if (opts.maxLength) v.push(Validators.maxLength(opts.maxLength));
      if (opts.presets?.length) v.push(presetValidator(opts.presets));
      else if (opts.formatMask) v.push(maskValidator(opts.formatMask));
      break;

    case 'LongText':
      if (opts.minLength) v.push(Validators.minLength(opts.minLength));
      if (opts.maxLength) v.push(Validators.maxLength(opts.maxLength));
      break;

    case 'Number':
      if (opts.min != null) v.push(Validators.min(opts.min));
      if (opts.max != null) v.push(Validators.max(opts.max));
      break;

    case 'Date':
      if (opts.minDate) v.push(minDateValidator(opts.minDate));
      if (opts.maxDate) v.push(maxDateValidator(opts.maxDate));
      break;
  }

  return v;
}

export function getCustomFieldErrorMessage(def: CustomFieldDefinition, errors: ValidationErrors): string {
  if (errors['serverError']) return errors['serverError'];
  if (errors['required'])    return `${def.label} é obrigatório.`;
  if (errors['minlength'])   return `${def.label} deve ter no mínimo ${def.validation?.minLength} caracteres.`;
  if (errors['maxlength'])   return `${def.label} deve ter no máximo ${def.validation?.maxLength} caracteres.`;
  if (errors['min'])         return `${def.label} deve ser maior ou igual a ${def.validation?.min}.`;
  if (errors['max'])         return `${def.label} deve ser menor ou igual a ${def.validation?.max}.`;
  if (errors['minDate'])     return `${def.label} deve ser a partir de ${def.validation?.minDate}.`;
  if (errors['maxDate'])     return `${def.label} deve ser até ${def.validation?.maxDate}.`;
  if (errors['invalidPreset']) return `${def.label}: formato inválido.`;
  if (errors['invalidMask'])   return `${def.label} não corresponde ao formato esperado.`;
  return `${def.label}: valor inválido.`;
}
```

---

## Directive — `input-mask.directive.ts`

Applied automatically by `CustomFieldInputComponent` when a `formatMask` or known preset mask is present. Inserts literal characters as the user types and prevents invalid character input.

```typescript
@Directive({
  selector: '[cfMask]',
  standalone: true
})
export class InputMaskDirective {
  cfMask = input.required<string>();  // e.g. 'DEPT-####' or '###.###.###-##' (CPF)

  @HostListener('input', ['$event'])
  onInput(event: InputEvent): void {
    const input = event.target as HTMLInputElement;
    input.value = applyMask(this.cfMask(), input.value.replace(/\D/g, ''));
  }
}

// Preset display masks (for input guidance, not validation logic)
export const PRESET_DISPLAY_MASKS: Record<string, string> = {
  cpf:            '###.###.###-##',
  cnpj:           '##.###.###/####-##',
  pis:            '###.#####.##-#',
  cnh:            '###########',
  phone_mobile:   '(##) #####-####',
  phone_landline: '(##) ####-####',
  cep:            '#####-###',
};
```

---

## Component: `CustomFieldInputComponent`

Standalone. Renders the right control for a single field definition. Used inside any entity form.

```typescript
// Inputs
definition = input.required<CustomFieldDefinition>();
control    = input.required<FormControl>();

// Computed
hasError = computed(() =>
  this.control().invalid && (this.control().dirty || this.control().touched)
);
errorMessage = computed(() => {
  const errors = this.control().errors;
  return errors ? getCustomFieldErrorMessage(this.definition(), errors) : '';
});

// Derives the mask to apply: preset display mask takes priority, then admin format mask
displayMask = computed(() => {
  const opts = this.definition().validation;
  if (opts?.presets?.length) return PRESET_DISPLAY_MASKS[opts.presets[0]] ?? null;
  return opts?.formatMask ?? null;
});
```

Template renders:
- `ShortText` → `<input type="text">` with `[cfMask]` if `displayMask()` is set
- `LongText` → `<textarea>`
- `Number` → `<input type="number">`
- `Date` → `<input type="date">`
- `Boolean` → toggle/checkbox bound to `"true"/"false"` string values
- `SingleChoice` → `<select>` with `definition().validation?.choices` as options
- Inline error rendered below each input when `hasError()` is true

---

## Component: `CustomFieldDisplayComponent`

Standalone. Read-only. Renders a list of `CustomFieldValueDto[]` in an "Informações Adicionais" section. Shows all values including disabled fields. Disabled fields get a `badge-status badge-amber` badge with text "Campo desativado".

```typescript
// Inputs
values   = input.required<CustomFieldValueDto[]>();
title    = input<string>('Informações Adicionais');

// Computed
activeValues   = computed(() => this.values().filter(v => !v.isDisabled));
disabledValues = computed(() => this.values().filter(v => v.isDisabled));
hasAny         = computed(() => this.values().length > 0);
```

Hidden entirely if `hasAny()` is false.

---

## Component: `CustomFieldFilterComponent`

Standalone. Renders a filter control appropriate to the field type. Emits `filterChange` when the user changes the filter value.

```typescript
definition  = input.required<CustomFieldDefinition>();
filterChange = output<{ definitionId: string; value: string | null; min?: string; max?: string }>();
```

Renders:
- `ShortText / LongText` → text input (ILIKE search)
- `Number` → two number inputs (min / max range)
- `Date` → two date inputs (from / to range)
- `Boolean` → three-state toggle (Yes / No / Any)
- `SingleChoice` → dropdown from `definition().validation?.choices`

---

## Admin Page: `custom-fields-admin.ts`

Route: `/admin/custom-fields` (child of the existing `/admin` guard, `roleGuard('Admin')`).

### State

```typescript
// Entity tab
selectedEntity = signal<CustomFieldTargetEntity>('Requisition');

// For Application entity: active scope sub-tab
selectedScope  = signal<CustomFieldTargetEntity>('Application_Applied');

// Resolved target = selectedEntity unless Application (then uses selectedScope)
resolvedTarget = computed<CustomFieldTargetEntity>(() =>
  this.selectedEntity() === 'Requisition' || this.selectedEntity() === 'JobPosting'
    ? this.selectedEntity()
    : this.selectedScope()
);

// Definitions loaded for the resolved target
definitions     = signal<CustomFieldDefinition[]>([]);
isLoading       = signal(false);
isBuilderOpen   = signal(false);
editingDef      = signal<CustomFieldDefinition | null>(null);   // null = create mode
availablePresets = signal<PresetGroup[]>([]);
```

### Effects

```typescript
effect(() => {
  // Re-load when resolvedTarget changes
  this.loadDefinitions(this.resolvedTarget());
});
```

### Template structure

```
Tabs: [Requisição] [Candidatura] [Vaga]
  └─ Application sub-tabs: [Global] [Triagem] [Entrevista] [Oferta]

Active Fields section
  ├─ "Add Field" button → opens DefinitionBuilderPanel
  └─ CdkDropList (draggable rows)
       └─ DefinitionRowComponent × N

Disabled Fields section (collapsed by default using <details>)
  └─ DefinitionRowComponent × N (re-enable + conditional delete)

DefinitionBuilderPanel (slide-over, shown when isBuilderOpen())
```

### Drag-and-drop reorder

Uses `@angular/cdk/drag-drop`. On `cdkDropListDropped`:

```typescript
onDrop(event: CdkDragDrop<CustomFieldDefinition[]>): void {
  const updated = [...this.definitions()];
  moveItemInArray(updated, event.previousIndex, event.currentIndex);
  this.definitions.set(updated);
  const orderedIds = updated.map(d => d.id);
  this.customFieldsService.reorderDefinitions(orderedIds).subscribe();
}
```

### Delete flow

```typescript
onDelete(def: CustomFieldDefinition): void {
  if (def.valueCount > 0) {
    // Show info dialog — only offers Disable
    this.showDisableOnlyDialog(def);
  } else {
    this.confirmDialog.open({
      title: 'Excluir campo',
      message: `Excluir "${def.label}" permanentemente? Esta ação não pode ser desfeita.`,
      confirmLabel: 'Excluir',
      isDangerous: true
    }).subscribe(confirmed => {
      if (confirmed) this.hardDelete(def.id);
    });
  }
}

private showDisableOnlyDialog(def: CustomFieldDefinition): void {
  this.confirmDialog.open({
    title: 'Campo com dados',
    message: `Este campo possui dados em ${def.valueCount} registro(s). Excluí-lo tornaria esses dados inacessíveis. Deseja desativá-lo?`,
    confirmLabel: 'Desativar campo',
    cancelLabel: 'Cancelar',
    isDangerous: false
  }).subscribe(confirmed => {
    if (confirmed) this.disable(def.id);
  });
}
```

All feedback (success / error) via `NotificationService`.

---

## Component: `DefinitionBuilderComponent`

Slide-over panel. Operates in **create** mode (no `def` input) or **edit** mode (receives existing `def`).

### Inputs / Outputs

```typescript
definition     = input<CustomFieldDefinition | null>(null);  // null = create mode
targetEntity   = input.required<CustomFieldTargetEntity>();
availablePresets = input.required<PresetGroup[]>();
saved          = output<CustomFieldDefinition>();
cancelled      = output<void>();
```

### State

```typescript
form = signal<FormGroup>(this.buildForm());

// Reactive to field type changes — controls which validation sections are shown
selectedType = computed<CustomFieldType>(() =>
  this.form().get('fieldType')?.value ?? 'ShortText'
);
hasPresetSelected = computed(() =>
  (this.form().get('presets')?.value ?? []).length > 0
);
isApplication = computed(() =>
  this.targetEntity().startsWith('Application')
);
candidateFacingAllowed = computed(() =>
  this.targetEntity() === 'Application_Global' ||
  this.targetEntity() === 'Application_Applied'
);
```

### Edit-mode locked fields

When `definition()` has `valueCount > 0`, the following controls are **disabled** in the form:
- `fieldType`
- `presets`
- `formatMask`

The UI renders a small notice below the locked fields: *"Este campo possui dados e não pode ter seu tipo ou formato alterado. Desative-o e crie um novo campo para mudanças estruturais."*

### Format Mask / Preset mutual exclusivity

```typescript
effect(() => {
  const hasPreset = this.hasPresetSelected();
  const maskControl = this.form().get('formatMask');
  hasPreset ? maskControl?.disable() : maskControl?.enable();
});
```

---

## Component: `PresetSelectorComponent`

```typescript
availablePresets = input.required<PresetGroup[]>();
selectedKeys     = model<string[]>([]);  // two-way binding via model()
disabled         = input<boolean>(false);
```

Renders presets as grouped checkboxes. Each group (`tag`) is a labelled fieldset. Toggling a preset adds/removes its key from `selectedKeys`. Multiple presets can be selected simultaneously (OR logic).

---

## Routing Changes — `app.routes.ts`

Add inside the `canActivate: [authGuard]` children array:

```typescript
{
  path: 'admin/custom-fields',
  canActivate: [roleGuard('Admin')],
  loadComponent: () =>
    import('./features/admin/custom-fields/custom-fields-admin')
      .then(m => m.CustomFieldsAdminComponent)
}
```

Add a "Campos Personalizados" navigation entry to `AdminSettingsComponent` alongside existing sections (Departments, Salary Bands, Cost Centers).

---

## Integration into Existing Forms

### Pattern (same for Requisition, Job Posting, Application)

Each entity form component:

1. Loads definitions on init in parallel with the entity: `forkJoin({ entity: ..., defs: customFieldsService.getDefinitions(target, { includeDisabled: false }) })`
2. Builds a `customFieldsGroup = customFieldFormService.buildFormGroup(defs, existingValues)` and adds it to the parent `FormGroup` as `customFields`.
3. Renders `<app-custom-field-input>` for each definition inside a "Informações Adicionais" fieldset.
4. On submit, calls `customFieldFormService.extractValues(customFieldsGroup, defs)` and includes in the request payload as `customFieldValues`.
5. On `422 ValidationProblem`, calls `customFieldFormService.applyServerErrors(errors, customFieldsGroup)` to surface field-level errors inline.

### Application (stage-aware)

The Application board / stage view additionally filters which definitions to show based on the current `ApplicationStatus`:

```typescript
visibleScopes = computed<CustomFieldTargetEntity[]>(() => {
  const status = this.application().status;
  const scopes: CustomFieldTargetEntity[] = ['Application_Global'];
  if (status === 'Applied' || status === 'Interview' || status === 'Offer' || status === 'Archive')
    scopes.push('Application_Applied');
  if (status === 'Interview' || status === 'Offer' || status === 'Archive')
    scopes.push('Application_Interview');
  if (status === 'Offer' || status === 'Archive')
    scopes.push('Application_Offer');
  return scopes;
});
```

Definitions and values for each scope are loaded in parallel via `forkJoin` and merged for display.

### Public Apply Form (`apply.ts`)

The job detail response already includes candidate-facing definitions. The apply form:
1. Reads `candidateFacingFields: CustomFieldDefinition[]` from the job posting response.
2. Builds `customFieldsGroup` via `CustomFieldFormService`.
3. Renders fields using `CustomFieldInputComponent`.
4. Includes `customFieldValues` in the `POST /api/public/jobs/{id}/apply` payload.

No authentication required — candidate-facing fields use the same components but in the public context.

---

## Detail Views

### `CustomFieldDisplayComponent` integration

- **Requisition detail**: load via `GET /api/requisitions/{id}` — response includes `customFields: CustomFieldValueDto[]` — pass to `<app-custom-field-display>`.
- **Candidate Profile / `application-detail`**: load values per scope, merge and pass to `<app-custom-field-display>`. Disabled field values are included and shown with the "Campo desativado" badge.

---

## Filtering Integration

Entity list pages (`requisition-list.ts`, `list/application-list.ts`) that support filtering:

1. Load active definitions for the entity on init.
2. Render a `<app-custom-field-filter>` for each definition inside the existing filter panel (collapsed by default, expanded on user action).
3. On `filterChange`, update a `activeFilters` signal and re-fetch the entity list with the updated query params.
4. Custom field filters are passed as query params alongside existing standard filters.

The filter state is held in a signal and reflected in the URL via `Router.navigate` with `queryParams` so filters are bookmarkable and shareable.
