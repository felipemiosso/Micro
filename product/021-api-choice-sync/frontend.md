# 021: API Choice Sync — Frontend Design

## Angular Services & Interfaces

### `custom-fields.service.ts` [Modified]
Add `disabledChoices` to `ValidationOptions` interface:
```typescript
export interface ValidationOptions {
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  minDate?: string;
  maxDate?: string;
  presets?: string[];
  formatMask?: string;
  choices?: string[];
  disabledChoices?: string[]; // [NEW] List of archived/disabled choices
}
```

---

## Form Input Component Changes

### `custom-field-input.ts` [Modified]
We must update the rendering behavior of `SingleChoice` inputs to handle retired options.

1. **Reactive State Syncing**: Track the current value of the form control reactively using a signal.
2. **Computed Choice List**: Create a computed list of choices to render. If the current control value is inside the `disabledChoices` list (indicating it is a historical record with a retired option), add that value to the options list. Otherwise, display only active choices.
3. **Retired Visual Indicator**: Append `(retired)` to the label of disabled choices and mark them as `disabled` in the HTML options to prevent new selections.

#### TypeScript Logic
```typescript
import { Component, input, computed, OnInit, OnDestroy, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
// ...

export class CustomFieldInputComponent implements OnInit, OnDestroy {
  definition = input.required<CustomFieldDefinition>();
  control    = input.required<FormControl>();

  currentControlValue = signal<string>('');
  private valueSub?: Subscription;

  ngOnInit() {
    this.currentControlValue.set(this.control().value);
    this.valueSub = this.control().valueChanges.subscribe(val => {
      this.currentControlValue.set(val);
    });
  }

  ngOnDestroy() {
    this.valueSub?.unsubscribe();
  }

  choicesToDisplay = computed(() => {
    const def = this.definition();
    const active = def.validation?.choices || [];
    const disabled = def.validation?.disabledChoices || [];
    const currentValue = this.currentControlValue();

    if (currentValue && disabled.includes(currentValue) && !active.includes(currentValue)) {
      return [...active, currentValue];
    }
    return active;
  });

  isChoiceDisabled(choice: string): boolean {
    const disabled = this.definition().validation?.disabledChoices || [];
    return disabled.includes(choice);
  }

  getChoiceLabel(choice: string): string {
    const disabled = this.definition().validation?.disabledChoices || [];
    return disabled.includes(choice) ? `${choice} (retired)` : choice;
  }
}
```

#### Template Update
```html
        @case ('SingleChoice') {
          <select
            [formControl]="control()"
            class="input-honeycomb"
          >
            <option value="">Select an option</option>
            @for (choice of choicesToDisplay(); track choice) {
              <option [value]="choice" [disabled]="isChoiceDisabled(choice)">
                {{ getChoiceLabel(choice) }}
              </option>
            }
          </select>
        }
      }

---

## Master Lookup Dropdown Filtering

### `admin.service.ts` [Modified]
Update `SalaryBand` and `CostCenter` interfaces to include `isActive`:
```typescript
export interface SalaryBand {
  id: string;
  name: string;
  minAmount: number;
  maxAmount: number;
  currency: string;
  isActive: boolean; // [NEW] Symmetrical IsActive state
}

export interface CostCenter {
  id: string;
  code: string;
  name: string;
  isActive: boolean; // [NEW] Symmetrical IsActive state
}
```

### Requisition Form Component (`requisition-form.ts`) [Modified]
Create computed signals for active departments, salary bands, and cost centers. These signals will output only the active items, plus the item currently selected in the form control if it happens to be inactive.

```typescript
  activeDepartments = computed(() => {
    const all = this.departments();
    const currentId = this.form.get('departmentId')?.value;
    return all.filter(d => d.isActive || d.id === currentId);
  });

  activeSalaryBands = computed(() => {
    const all = this.salaryBands();
    const currentId = this.form.get('salaryBandId')?.value;
    return all.filter(b => b.isActive || b.id === currentId);
  });

  activeCostCenters = computed(() => {
    const all = this.costCenters();
    const currentId = this.form.get('costCenterId')?.value;
    return all.filter(c => c.isActive || c.id === currentId);
  });
```

### Requisition Form Template (`requisition-form.html`) [Modified]
Update `<select>` options to iterate over the new computed properties and display `(inactive)` markers:

```html
      <!-- Department -->
      <div class="flex flex-col gap-2">
        <label for="departmentId" class="text-xs font-bold text-ink uppercase tracking-widest">Department</label>
        <select id="departmentId" formControlName="departmentId" class="input-honeycomb">
          <option value="">Select Department</option>
          @for (d of activeDepartments(); track d.id) {
            <option [value]="d.id" [disabled]="!d.isActive">
              {{ d.name }}{{ d.isActive ? '' : ' (inactive)' }}
            </option>
          }
        </select>
      </div>

      <!-- Cost Center -->
      <div class="flex flex-col gap-2">
        <label for="costCenterId" class="text-xs font-bold text-ink uppercase tracking-widest">Cost Center</label>
        <select id="costCenterId" formControlName="costCenterId" class="input-honeycomb">
          <option value="">Select Cost Center</option>
          @for (cc of activeCostCenters(); track cc.id) {
            <option [value]="cc.id" [disabled]="!cc.isActive">
              {{ cc.code }} - {{ cc.name }}{{ cc.isActive ? '' : ' (inactive)' }}
            </option>
          }
        </select>
      </div>

      <!-- Salary Band -->
      <div class="flex flex-col gap-2">
        <label for="salaryBandId" class="text-xs font-bold text-ink uppercase tracking-widest">Salary Band</label>
        <select id="salaryBandId" formControlName="salaryBandId" class="input-honeycomb">
          <option value="">Select Salary Band</option>
          @for (b of activeSalaryBands(); track b.id) {
            <option [value]="b.id" [disabled]="!b.isActive">
              {{ b.name }} ({{ b.minAmount | number }} - {{ b.maxAmount | number }}){{ b.isActive ? '' : ' (inactive)' }}
            </option>
          }
        </select>
      </div>
```
