import { Component, input, model } from '@angular/core';
import { PresetGroup } from '../../../../core/services/custom-fields.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-preset-selector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-4">
      @for (group of availablePresets(); track group.tag) {
        <fieldset class="border border-slate-200 dark:border-slate-800 rounded-md p-3">
          <legend class="text-xs font-semibold text-slate-500 dark:text-slate-400 px-2 bg-slate-50 dark:bg-slate-900 rounded">
            {{ group.tag }}
          </legend>
          <div class="grid grid-cols-2 gap-2 mt-2">
            @for (preset of group.presets; track preset.key) {
              <label class="flex items-center space-x-2 text-sm text-slate-700 dark:text-slate-300 cursor-pointer">
                <input
                  type="checkbox"
                  [disabled]="disabled()"
                  [checked]="isSelected(preset.key)"
                  (change)="togglePreset(preset.key)"
                  class="rounded border-slate-300 dark:border-slate-700 text-honeycomb-600 focus:ring-honeycomb-500 bg-white dark:bg-slate-900"
                />
                <span>{{ preset.label }}</span>
              </label>
            }
          </div>
        </fieldset>
      }
    </div>
  `
})
export class PresetSelectorComponent {
  availablePresets = input.required<PresetGroup[]>();
  selectedKeys     = model<string[]>([]);
  disabled         = input<boolean>(false);

  isSelected(key: string): boolean {
    return this.selectedKeys().includes(key);
  }

  togglePreset(key: string): void {
    if (this.disabled()) return;
    const current = this.selectedKeys();
    if (current.includes(key)) {
      this.selectedKeys.set([]);
    } else {
      this.selectedKeys.set([key]);
    }
  }
}
