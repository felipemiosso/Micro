import { Component, input, computed } from '@angular/core';
import { CustomFieldValueDto } from '../../services/custom-fields.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-custom-field-display',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (hasAny()) {
      <div class="mt-6 border-t border-slate-200 dark:border-slate-800 pt-6">
        <h3 class="text-sm font-semibold text-slate-900 dark:text-slate-100 mb-4">{{ title() }}</h3>

        <dl class="grid grid-cols-1 gap-x-4 gap-y-4 sm:grid-cols-2">
          @for (val of values(); track val.definitionId) {
            <div class="sm:col-span-1">
              <dt class="text-xs font-medium text-slate-500 dark:text-slate-400 flex items-center space-x-2">
                <span>{{ val.label }}</span>
                @if (val.isDisabled) {
                  <span class="badge-status badge-zinc">Field disabled</span>
                }
              </dt>
              <dd class="mt-1 text-sm text-slate-900 dark:text-slate-100 whitespace-pre-line">
                @if (val.fieldType === 'Boolean') {
                  {{ val.value === 'true' ? 'Yes' : val.value === 'false' ? 'No' : (val.value || '-') }}
                } @else {
                  {{ val.value || '-' }}
                }
              </dd>
            </div>
          }
        </dl>
      </div>
    }
  `
})
export class CustomFieldDisplayComponent {
  values   = input.required<CustomFieldValueDto[]>();
  title    = input<string>('Additional Information');

  activeValues   = computed(() => this.values().filter(v => !v.isDisabled));
  disabledValues = computed(() => this.values().filter(v => v.isDisabled));
  hasAny         = computed(() => this.values().length > 0);
}
