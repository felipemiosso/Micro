import { Component, input, output, computed, effect, signal, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CustomFieldDefinition, CustomFieldTargetEntity, CustomFieldType, PresetGroup, CustomFieldsService } from '../../../../core/services/custom-fields.service';
import { PresetSelectorComponent } from './preset-selector';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-definition-builder',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PresetSelectorComponent],
  template: `
    <div class="fixed inset-0 z-50 overflow-hidden" aria-labelledby="slide-over-title" role="dialog" aria-modal="true">
      <div class="absolute inset-0 overflow-hidden bg-slate-900/50 backdrop-blur-sm transition-opacity" (click)="cancelled.emit()"></div>

      <div class="absolute inset-y-0 right-0 flex max-w-full pl-10">
        <div class="w-screen max-w-md transform transition-all duration-300 ease-in-out bg-white dark:bg-slate-900 border-l border-slate-200 dark:border-slate-800 shadow-xl flex flex-col h-full text-slate-900 dark:text-slate-100">

          <!-- Header -->
          <div class="px-6 py-5 border-b border-slate-200 dark:border-slate-800 flex items-center justify-between">
            <h2 class="text-lg font-medium text-slate-900 dark:text-slate-100" id="slide-over-title">
              {{ definition() ? 'Edit Field' : 'New Custom Field' }}
            </h2>
            <button type="button" class="rounded-md text-slate-400 hover:text-slate-500 focus:outline-none" (click)="cancelled.emit()">
              <span class="sr-only">Close</span>
              <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <!-- Form Content -->
          <form [formGroup]="form" class="flex-1 overflow-y-auto px-6 py-6 space-y-6">

            @if (definition() && definition()!.valueCount > 0) {
              <div class="bg-amber-50 dark:bg-amber-950/40 border-l-4 border-amber-500 p-3 rounded-r-md">
                <p class="text-xs text-amber-900 dark:text-amber-200 font-medium">
                  This field has values (in {{ definition()!.valueCount }} record(s)) and cannot have its type or format updated. Disable it and create a new field for structural changes.
                </p>
              </div>
            }

            <!-- Label -->
            <div>
              <label class="block text-sm font-medium text-slate-700 dark:text-slate-300">Label / Name <span class="text-rose-500">*</span></label>
              <input type="text" formControlName="label" class="mt-1 input-honeycomb" placeholder="e.g. CPF, Date of Birth" />
              @if (form.get('label')?.invalid && form.get('label')?.touched) {
                <p class="mt-1 text-xs text-rose-600 dark:text-rose-400">Label is required and must be at most 200 characters.</p>
              }
            </div>

            <!-- Help Text -->
            <div>
              <label class="block text-sm font-medium text-slate-700 dark:text-slate-300">Help Text / Placeholder</label>
              <input type="text" formControlName="helpText" class="mt-1 input-honeycomb" placeholder="e.g. Enter CPF (numbers only)" />
              @if (form.get('helpText')?.invalid && form.get('helpText')?.touched) {
                <p class="mt-1 text-xs text-rose-600 dark:text-rose-400">Maximum 500 characters.</p>
              }
            </div>

            <!-- Field Type -->
            <div>
              <label class="block text-sm font-medium text-slate-700 dark:text-slate-300">Field Type <span class="text-rose-500">*</span></label>
              <select formControlName="fieldType" class="mt-1 input-honeycomb">
                <option value="ShortText">Short Text</option>
                <option value="LongText">Long Text</option>
                <option value="Number">Number</option>
                <option value="Date">Date</option>
                <option value="Boolean">Boolean (Yes/No)</option>
                <option value="SingleChoice">Single Choice</option>
              </select>
            </div>

            <!-- Required, Candidate Facing & Global Toggles -->
            <div class="space-y-3 pt-2">
              <label class="flex items-center space-x-2 text-sm text-slate-700 dark:text-slate-300 cursor-pointer">
                <input type="checkbox" formControlName="isGlobal" class="rounded border-slate-300 dark:border-slate-700 text-honeycomb-600 focus:ring-honeycomb-500 bg-white dark:bg-slate-900 disabled:opacity-50" />
                <span>Global Field (automatically active on all instances)</span>
              </label>

              <label class="flex items-center space-x-2 text-sm text-slate-700 dark:text-slate-300 cursor-pointer">
                <input type="checkbox" formControlName="isRequired" class="rounded border-slate-300 dark:border-slate-700 text-honeycomb-600 focus:ring-honeycomb-500 bg-white dark:bg-slate-900" />
                <span>Required Field</span>
              </label>

              @if (candidateFacingAllowed()) {
                <label class="flex items-center space-x-2 text-sm text-slate-700 dark:text-slate-300 cursor-pointer">
                  <input type="checkbox" formControlName="isCandidateFacing" class="rounded border-slate-300 dark:border-slate-700 text-honeycomb-600 focus:ring-honeycomb-500 bg-white dark:bg-slate-900" />
                  <span>Candidate Facing (on public apply form)</span>
                </label>
              }
            </div>

            <!-- VALIDATIONS DYNAMIC SECTION -->
            <div class="border-t border-slate-200 dark:border-slate-800 pt-4 space-y-4">
              <h3 class="text-sm font-medium text-slate-900 dark:text-slate-100">Validation Rules</h3>

              <!-- Validation: Text (Short/Long) -->
              @if (selectedType() === 'ShortText' || selectedType() === 'LongText') {
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-xs font-medium text-slate-700 dark:text-slate-300">Min Characters</label>
                    <input type="number" formControlName="minLength" class="mt-1 input-honeycomb !py-1.5 !px-3 !text-xs" />
                  </div>
                  <div>
                    <label class="block text-xs font-medium text-slate-700 dark:text-slate-300">Max Characters</label>
                    <input type="number" formControlName="maxLength" class="mt-1 input-honeycomb !py-1.5 !px-3 !text-xs" />
                  </div>
                </div>
              }

              <!-- Validation: ShortText Special Formatting (Presets vs Mask) -->
              @if (selectedType() === 'ShortText') {
                <div class="space-y-4">
                  <div>
                    <label class="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">Format Mask (e.g. ####-##-## or AAA-####)</label>
                    <input type="text" formControlName="formatMask" class="input-honeycomb !py-1.5 !px-3 !text-xs" placeholder="Use # for digits, A for letters, X for both" />
                  </div>

                  <div>
                    <label class="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-2">Preset Validators (Presets)</label>
                    <app-preset-selector [availablePresets]="availablePresets()" [disabled]="isPresetSelectorDisabled()" [selectedKeys]="presets()" (selectedKeysChange)="onPresetsChange($event)"></app-preset-selector>
                  </div>
                </div>
              }

              <!-- Validation: Number -->
              @if (selectedType() === 'Number') {
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-xs font-medium text-slate-700 dark:text-slate-300">Minimum Value</label>
                    <input type="number" formControlName="min" step="any" class="mt-1 input-honeycomb !py-1.5 !px-3 !text-xs" />
                  </div>
                  <div>
                    <label class="block text-xs font-medium text-slate-700 dark:text-slate-300">Maximum Value</label>
                    <input type="number" formControlName="max" step="any" class="mt-1 input-honeycomb !py-1.5 !px-3 !text-xs" />
                  </div>
                </div>
              }

              <!-- Validation: Date -->
              @if (selectedType() === 'Date') {
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-xs font-medium text-slate-700 dark:text-slate-300">Minimum Date</label>
                    <input type="date" formControlName="minDate" class="mt-1 input-honeycomb !py-1.5 !px-3 !text-xs" />
                  </div>
                  <div>
                    <label class="block text-xs font-medium text-slate-700 dark:text-slate-300">Maximum Date</label>
                    <input type="date" formControlName="maxDate" class="mt-1 input-honeycomb !py-1.5 !px-3 !text-xs" />
                  </div>
                </div>
              }

              <!-- Validation: SingleChoice -->
              @if (selectedType() === 'SingleChoice') {
                <div>
                  <label class="block text-xs font-medium text-slate-700 dark:text-slate-300">Choices (one per line) <span class="text-rose-500">*</span></label>
                  <textarea formControlName="choices" rows="4" class="mt-1 input-honeycomb !py-1.5 !px-3 !text-xs" placeholder="Option A&#10;Option B&#10;Option C"></textarea>
                  @if (form.get('choices')?.invalid && form.get('choices')?.touched) {
                    <p class="mt-1 text-xs text-rose-600 dark:text-rose-400">Choices are required for single choice.</p>
                  }
                </div>
              }
            </div>
          </form>

          <!-- Footer Actions -->
          <div class="px-6 py-4 bg-slate-50 dark:bg-slate-900/50 border-t border-slate-200 dark:border-slate-800 flex items-center justify-end space-x-3">
            <button type="button" class="btn-secondary !py-2 !px-4 text-sm" (click)="cancelled.emit()">
              Cancel
            </button>
            <button type="submit" [disabled]="form.invalid || isSubmitting" (click)="onSubmit()" class="btn-primary !py-2 !px-4 text-sm">
              {{ isSubmitting ? 'Saving...' : 'Save' }}
            </button>
          </div>

        </div>
      </div>
    </div>
  `
})
export class DefinitionBuilderComponent {
  definition       = input<CustomFieldDefinition | null>(null);
  targetEntity     = input.required<CustomFieldTargetEntity>();
  availablePresets = input.required<PresetGroup[]>();
  saved            = output<CustomFieldDefinition>();
  cancelled        = output<void>();

  private fb = inject(FormBuilder);
  private customFieldsService = inject(CustomFieldsService);

  form!: FormGroup;
  isSubmitting = false;
  presets = signal<string[]>([]);

  selectedType = signal<CustomFieldType>('ShortText');
  hasValueDataLocked = computed(() => {
    const def = this.definition();
    return def ? def.valueCount > 0 : false;
  });

  isPresetSelectorDisabled = computed(() => {
    if (this.hasValueDataLocked()) return true;
    const formatMask = this.form ? this.form.get('formatMask')?.value : '';
    return !!formatMask;
  });

  candidateFacingAllowed = computed(() =>
    this.targetEntity() === 'Application_Global' || this.targetEntity() === 'Application_Applied'
  );

  constructor() {
    this.form = this.fb.group({
      label: ['', [Validators.required, Validators.maxLength(200)]],
      helpText: ['', [Validators.maxLength(500)]],
      fieldType: ['ShortText', Validators.required],
      isRequired: [false],
      isCandidateFacing: [false],
      isGlobal: [true],
      minLength: [null],
      maxLength: [null],
      min: [null],
      max: [null],
      minDate: [null],
      maxDate: [null],
      formatMask: [''],
      choices: ['']
    });

    this.form.get('fieldType')?.valueChanges.subscribe(val => {
      this.selectedType.set(val);
      this.updateConditionalValidators(val);
    });

    this.form.get('formatMask')?.valueChanges.subscribe(val => {
      if (val) {
        this.presets.set([]);
      }
    });

    effect(() => {
      const def = this.definition();
      if (def) {
        this.form.patchValue({
          label: def.label,
          helpText: def.helpText || '',
          fieldType: def.fieldType,
          isRequired: def.isRequired,
          isCandidateFacing: def.isCandidateFacing,
          isGlobal: def.isGlobal,
          minLength: def.validation?.minLength || null,
          maxLength: def.validation?.maxLength || null,
          min: def.validation?.min || null,
          max: def.validation?.max || null,
          minDate: def.validation?.minDate || null,
          maxDate: def.validation?.maxDate || null,
          formatMask: def.validation?.formatMask || '',
          choices: def.validation?.choices?.join('\n') || ''
        });

        this.presets.set(def.validation?.presets || []);
        this.selectedType.set(def.fieldType);

        if (def.valueCount > 0) {
          this.form.get('fieldType')?.disable();
          this.form.get('formatMask')?.disable();
          this.form.get('isGlobal')?.disable();
        } else {
          this.form.get('fieldType')?.enable();
          this.form.get('formatMask')?.enable();
          this.form.get('isGlobal')?.enable();
        }
      } else {
        this.form.reset({
          fieldType: 'ShortText',
          isRequired: false,
          isCandidateFacing: false,
          isGlobal: true,
          formatMask: '',
          choices: ''
        });
        this.presets.set([]);
        this.selectedType.set('ShortText');
        this.form.get('fieldType')?.enable();
        this.form.get('formatMask')?.enable();
        this.form.get('isGlobal')?.enable();
      }
    });

    effect(() => {
      const presetsCount = this.presets().length;
      const maskControl = this.form.get('formatMask');
      const minLengthControl = this.form.get('minLength');
      const maxLengthControl = this.form.get('maxLength');

      if (presetsCount > 0) {
        maskControl?.setValue('', { emitEvent: false });
        maskControl?.disable({ emitEvent: false });

        minLengthControl?.setValue(null, { emitEvent: false });
        minLengthControl?.disable({ emitEvent: false });

        maxLengthControl?.setValue(null, { emitEvent: false });
        maxLengthControl?.disable({ emitEvent: false });
      } else {
        if (!this.hasValueDataLocked()) {
          maskControl?.enable({ emitEvent: false });
        }
        minLengthControl?.enable({ emitEvent: false });
        maxLengthControl?.enable({ emitEvent: false });
      }
    });
  }

  updateConditionalValidators(type: CustomFieldType): void {
    const choicesControl = this.form.get('choices');
    if (type === 'SingleChoice') {
      choicesControl?.setValidators([Validators.required]);
    } else {
      choicesControl?.clearValidators();
    }
    choicesControl?.updateValueAndValidity();
  }

  onPresetsChange(newPresets: string[]): void {
    this.presets.set(newPresets);
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting) return;

    this.isSubmitting = true;
    const rawVal = this.form.getRawValue();

    const validation: any = {};
    const type = rawVal.fieldType;

    if (type === 'ShortText' || type === 'LongText') {
      if (rawVal.minLength != null) validation.minLength = rawVal.minLength;
      if (rawVal.maxLength != null) validation.maxLength = rawVal.maxLength;
      if (type === 'ShortText') {
        if (this.presets().length > 0) validation.presets = this.presets();
        else if (rawVal.formatMask) validation.formatMask = rawVal.formatMask;
      }
    } else if (type === 'Number') {
      if (rawVal.min != null) validation.min = rawVal.min;
      if (rawVal.max != null) validation.max = rawVal.max;
    } else if (type === 'Date') {
      if (rawVal.minDate) validation.minDate = rawVal.minDate;
      if (rawVal.maxDate) validation.maxDate = rawVal.maxDate;
    } else if (type === 'SingleChoice') {
      const lines = rawVal.choices ? rawVal.choices.split('\n').map((c: string) => c.trim()).filter((c: string) => c.length > 0) : [];
      validation.choices = lines;
    }

    const hasValidation = Object.keys(validation).length > 0;

    const requestPayload = {
      label: rawVal.label,
      helpText: rawVal.helpText || null,
      isRequired: rawVal.isRequired,
      isCandidateFacing: this.candidateFacingAllowed() ? rawVal.isCandidateFacing : false,
      isGlobal: rawVal.isGlobal,
      validation: hasValidation ? validation : null
    };

    const def = this.definition();
    if (def) {
      this.customFieldsService.updateDefinition(def.id, requestPayload).subscribe({
        next: (savedDef) => {
          this.isSubmitting = false;
          this.saved.emit(savedDef);
        },
        error: () => {
          this.isSubmitting = false;
        }
      });
    } else {
      const createPayload = {
        ...requestPayload,
        targetEntity: this.targetEntity(),
        fieldType: rawVal.fieldType
      };
      this.customFieldsService.createDefinition(createPayload).subscribe({
        next: (savedDef) => {
          this.isSubmitting = false;
          this.saved.emit(savedDef);
        },
        error: () => {
          this.isSubmitting = false;
        }
      });
    }
  }
}
