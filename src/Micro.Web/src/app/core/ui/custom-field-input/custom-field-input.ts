import { Component, input, computed } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CustomFieldDefinition } from '../../services/custom-fields.service';
import { InputMaskDirective, PRESET_DISPLAY_MASKS } from '../input-mask.directive';
import { getCustomFieldErrorMessage } from '../../validators/custom-field.validators';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-custom-field-input',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, InputMaskDirective],
  template: `
    <div class="mb-4">
      <label class="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
        {{ definition().label }}
        @if (definition().isRequired) {
          <span class="text-rose-500 font-semibold">*</span>
        }
      </label>

      @switch (definition().fieldType) {
        @case ('ShortText') {
          @if (displayMask()) {
            <input
              type="text"
              [formControl]="control()"
              [cfMask]="displayMask()!"
              [placeholder]="displayMask()!"
              class="input-honeycomb"
            />
          } @else {
            <input
              type="text"
              [formControl]="control()"
              [placeholder]="definition().helpText || ''"
              class="input-honeycomb"
            />
          }
        }
        @case ('LongText') {
          <textarea
            [formControl]="control()"
            [placeholder]="definition().helpText || ''"
            rows="3"
            class="input-honeycomb"
          ></textarea>
        }
        @case ('Number') {
          <input
            type="number"
            [formControl]="control()"
            [placeholder]="definition().helpText || ''"
            class="input-honeycomb"
          />
        }
        @case ('Date') {
          <input
            type="date"
            [formControl]="control()"
            class="input-honeycomb"
          />
        }
        @case ('Boolean') {
          <div class="flex items-center space-x-4 py-2">
            <label class="flex items-center cursor-pointer text-sm text-slate-700 dark:text-slate-300">
              <input
                type="radio"
                [formControl]="control()"
                value="true"
                class="h-4 w-4 text-honeycomb-600 focus:ring-honeycomb-500 border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900"
              />
              <span class="ml-2">Yes</span>
            </label>
            <label class="flex items-center cursor-pointer text-sm text-slate-700 dark:text-slate-300">
              <input
                type="radio"
                [formControl]="control()"
                value="false"
                class="h-4 w-4 text-honeycomb-600 focus:ring-honeycomb-500 border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900"
              />
              <span class="ml-2">No</span>
            </label>
          </div>
        }
        @case ('SingleChoice') {
          <select
            [formControl]="control()"
            class="input-honeycomb"
          >
            <option value="">Select an option</option>
            @for (choice of definition().validation?.choices || []; track choice) {
              <option [value]="choice">{{ choice }}</option>
            }
          </select>
        }
      }

      @if (definition().helpText && definition().fieldType !== 'ShortText' && definition().fieldType !== 'LongText' && definition().fieldType !== 'Number') {
        <p class="mt-1 text-xs text-slate-500 dark:text-slate-400">{{ definition().helpText }}</p>
      }

      @if (hasError()) {
        <p class="mt-1 text-xs text-rose-600 dark:text-rose-400 font-medium">{{ errorMessage() }}</p>
      }
    </div>
  `
})
export class CustomFieldInputComponent {
  definition = input.required<CustomFieldDefinition>();
  control    = input.required<FormControl>();

  hasError = computed(() =>
    this.control().invalid && (this.control().dirty || this.control().touched)
  );

  errorMessage = computed(() => {
    const errors = this.control().errors;
    return errors ? getCustomFieldErrorMessage(this.definition(), errors) : '';
  });

  displayMask = computed(() => {
    const opts = this.definition().validation;
    if (opts?.presets?.length) return PRESET_DISPLAY_MASKS[opts.presets[0]] ?? null;
    return opts?.formatMask ?? null;
  });
}
