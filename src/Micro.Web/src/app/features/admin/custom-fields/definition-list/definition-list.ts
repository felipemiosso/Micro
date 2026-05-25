import { Component, input, output, computed } from '@angular/core';
import { CustomFieldDefinition } from '../../../../core/services/custom-fields.service';
import { DefinitionRowComponent } from './definition-row';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-definition-list',
  standalone: true,
  imports: [CommonModule, DragDropModule, DefinitionRowComponent],
  template: `
    <div class="space-y-6">

      <!-- Active fields (cdkDropList) -->
      <div>
        <div class="flex items-center justify-between mb-3">
          <h3 class="text-sm font-semibold text-slate-900 dark:text-slate-100">Active Fields</h3>
          <span class="text-xs text-slate-500 dark:text-slate-400 font-mono">{{ activeDefinitions().length }} field(s)</span>
        </div>

        @if (activeDefinitions().length === 0) {
          <div class="p-8 border-2 border-dashed border-slate-300 dark:border-slate-800 rounded-lg text-center">
            <p class="text-sm text-slate-500 dark:text-slate-400">No active custom fields registered for this scope.</p>
          </div>
        } @else {
          <div
            cdkDropList
            (cdkDropListDropped)="onDrop($event)"
            class="space-y-2"
          >
            @for (def of activeDefinitions(); track def.id) {
              <div cdkDrag class="bg-white dark:bg-slate-900 rounded-lg shadow-sm">
                <app-definition-row
                  [definition]="def"
                  [reorderable]="true"
                  (edit)="edit.emit($event)"
                  (toggleDisable)="toggleDisable.emit($event)"
                  (delete)="delete.emit($event)"
                ></app-definition-row>
              </div>
            }
          </div>
        }
      </div>

      <!-- Disabled fields (details collapsed) -->
      @if (disabledDefinitions().length > 0) {
        <details class="border border-slate-200 dark:border-slate-800 rounded-lg bg-slate-50/50 dark:bg-slate-900/50 p-4 group">
          <summary class="flex items-center justify-between cursor-pointer font-medium text-sm text-slate-700 dark:text-slate-300 select-none">
            <span class="flex items-center space-x-2">
              <svg class="h-4 w-4 transition-transform group-open:rotate-90" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
              </svg>
              <span>Disabled Fields ({{ disabledDefinitions().length }})</span>
            </span>
            <span class="text-xs text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/20 px-2.5 py-0.5 rounded-full border border-amber-200/55">Inactive</span>
          </summary>

          <div class="space-y-2 mt-4">
            @for (def of disabledDefinitions(); track def.id) {
              <app-definition-row
                [definition]="def"
                [reorderable]="false"
                (edit)="edit.emit($event)"
                (toggleDisable)="toggleDisable.emit($event)"
                (delete)="delete.emit($event)"
              ></app-definition-row>
            }
          </div>
        </details>
      }

    </div>
  `
})
export class DefinitionListComponent {
  definitions = input.required<CustomFieldDefinition[]>();

  edit          = output<CustomFieldDefinition>();
  toggleDisable = output<CustomFieldDefinition>();
  delete        = output<CustomFieldDefinition>();
  reorder       = output<string[]>();

  activeDefinitions = computed(() =>
    this.definitions().filter(d => !d.isDisabled).sort((a, b) => a.order - b.order)
  );

  disabledDefinitions = computed(() =>
    this.definitions().filter(d => d.isDisabled).sort((a, b) => a.order - b.order)
  );

  onDrop(event: CdkDragDrop<any[]>): void {
    const activeList = [...this.activeDefinitions()];
    moveItemInArray(activeList, event.previousIndex, event.currentIndex);

    const newOrderedIds = [
      ...activeList.map(d => d.id),
      ...this.disabledDefinitions().map(d => d.id)
    ];

    this.reorder.emit(newOrderedIds);
  }
}
