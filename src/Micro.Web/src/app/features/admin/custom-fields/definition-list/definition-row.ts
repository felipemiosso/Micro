import { Component, input, output } from '@angular/core';
import { CustomFieldDefinition } from '../../../../core/services/custom-fields.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-definition-row',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex items-center justify-between p-4 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-lg shadow-sm hover:shadow-md transition-shadow">
      <div class="flex items-center space-x-3">
        <!-- Drag Handle -->
        @if (reorderable()) {
          <div class="cursor-grab text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 active:cursor-grabbing p-1" cdkDragHandle>
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 9h16.5m-16.5 6.75h16.5" />
            </svg>
          </div>
        }

        <!-- Info -->
        <div>
          <div class="flex items-center space-x-2">
            <span class="font-medium text-slate-900 dark:text-slate-100">{{ definition().label }}</span>
            <span class="text-xs text-slate-500 dark:text-slate-400 font-mono">({{ definition().fieldType }})</span>
          </div>
          @if (definition().helpText) {
            <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">{{ definition().helpText }}</p>
          }
          <!-- Badges -->
          <div class="flex flex-wrap gap-1.5 mt-2">
            @if (definition().isRequired) {
              <span class="badge-status badge-red text-[10px] px-1.5 py-0.5">Required</span>
            }
            @if (definition().isCandidateFacing) {
              <span class="badge-status badge-green text-[10px] px-1.5 py-0.5">Candidate</span>
            }
            @if (definition().isDisabled) {
              <span class="badge-status badge-zinc text-[10px] px-1.5 py-0.5">Disabled</span>
            }
            @if (definition().valueCount > 0) {
              <span class="inline-flex items-center px-1.5 py-0.5 rounded-full text-[10px] font-medium bg-slate-100 dark:bg-slate-800 text-slate-800 dark:text-slate-200">
                {{ definition().valueCount }} value(s)
              </span>
            }
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div class="flex items-center space-x-2">
        <button
          type="button"
          (click)="edit.emit(definition())"
          class="p-1.5 text-slate-600 dark:text-slate-400 hover:text-honeycomb-600 dark:hover:text-honeycomb-400 rounded-md hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
          title="Edit"
        >
          <svg class="h-4.5 w-4.5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="m16.862 4.487 1.687-1.688a1.875 1.875 0 1 1 2.652 2.652L10.582 16.07a4.5 4.5 0 0 1-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 0 1 1.13-1.897l8.932-8.931Zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0 1 15.75 21H5.25A2.25 2.25 0 0 1 3 18.75V8.25A2.25 2.25 0 0 1 5.25 6H10" />
          </svg>
        </button>

        <button
          type="button"
          (click)="toggleDisable.emit(definition())"
          class="p-1.5 rounded-md hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
          [class]="definition().isDisabled ? 'text-emerald-600 dark:text-emerald-400 hover:text-emerald-700' : 'text-amber-600 dark:text-amber-400 hover:text-amber-700'"
          [title]="definition().isDisabled ? 'Enable' : 'Disable'"
        >
          @if (definition().isDisabled) {
            <svg class="h-4.5 w-4.5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75 11.25 15 15 9.75M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
            </svg>
          } @else {
            <svg class="h-4.5 w-4.5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M18.364 18.364A9 9 0 0 0 5.636 5.636m12.728 12.728A9 9 0 0 0 5.636 5.636m12.728 12.728L5.636 5.636" />
            </svg>
          }
        </button>

        <button
          type="button"
          (click)="delete.emit(definition())"
          class="p-1.5 text-slate-600 dark:text-slate-400 hover:text-rose-600 dark:hover:text-rose-400 rounded-md hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
          title="Delete"
        >
          <svg class="h-4.5 w-4.5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
          </svg>
        </button>
      </div>
    </div>
  `
})
export class DefinitionRowComponent {
  definition = input.required<CustomFieldDefinition>();
  reorderable = input<boolean>(true);

  edit          = output<CustomFieldDefinition>();
  toggleDisable = output<CustomFieldDefinition>();
  delete        = output<CustomFieldDefinition>();
}
