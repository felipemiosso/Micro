import { Component, input, output } from '@angular/core';
import { CustomFieldDefinition } from '../../services/custom-fields.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-custom-field-filter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="mb-4">
      <label class="block text-xs font-medium text-slate-500 dark:text-slate-400 mb-1">
        {{ definition().label }}
      </label>

      @switch (definition().fieldType) {
        @case ('ShortText') {
          <input
            type="text"
            [(ngModel)]="textValue"
            (ngModelChange)="onTextChange($event)"
            placeholder="Search..."
            class="input-honeycomb !py-1.5 !px-3 !text-xs"
          />
        }
        @case ('LongText') {
          <input
            type="text"
            [(ngModel)]="textValue"
            (ngModelChange)="onTextChange($event)"
            placeholder="Search..."
            class="input-honeycomb !py-1.5 !px-3 !text-xs"
          />
        }
        @case ('Number') {
          <div class="flex items-center space-x-2 w-full">
            <input
              type="number"
              [(ngModel)]="minVal"
              (ngModelChange)="onRangeChange()"
              placeholder="Min"
              class="input-honeycomb !w-1/2 !py-1.5 !px-2 !text-xs"
            />
            <span class="text-slate-400 text-xs">-</span>
            <input
              type="number"
              [(ngModel)]="maxVal"
              (ngModelChange)="onRangeChange()"
              placeholder="Max"
              class="input-honeycomb !w-1/2 !py-1.5 !px-2 !text-xs"
            />
          </div>
        }
        @case ('Date') {
          <div class="flex items-center space-x-2 w-full">
            <input
              type="date"
              [(ngModel)]="minVal"
              (ngModelChange)="onRangeChange()"
              class="input-honeycomb !w-[calc(50%-4px)] !py-1.5 !px-1 !text-xs"
            />
            <span class="text-slate-400 text-xs">-</span>
            <input
              type="date"
              [(ngModel)]="maxVal"
              (ngModelChange)="onRangeChange()"
              class="input-honeycomb !w-[calc(50%-4px)] !py-1.5 !px-1 !text-xs"
            />
          </div>
        }
        @case ('Boolean') {
          <select
            [(ngModel)]="booleanVal"
            (ngModelChange)="onSelectChange($event)"
            class="input-honeycomb !py-1.5 !px-3 !text-xs"
          >
            <option value="">Any</option>
            <option value="true">Yes</option>
            <option value="false">No</option>
          </select>
        }
        @case ('SingleChoice') {
          <select
            [(ngModel)]="choiceVal"
            (ngModelChange)="onSelectChange($event)"
            class="input-honeycomb !py-1.5 !px-3 !text-xs"
          >
            <option value="">Any</option>
            @for (choice of definition().validation?.choices || []; track choice) {
              <option [value]="choice">{{ choice }}</option>
            }
          </select>
        }
      }
    </div>
  `
})
export class CustomFieldFilterComponent {
  definition = input.required<CustomFieldDefinition>();
  filterChange = output<{ definitionId: string; value: string | null; min?: string; max?: string }>();

  textValue: string = '';
  minVal: string = '';
  maxVal: string = '';
  booleanVal: string = '';
  choiceVal: string = '';

  onTextChange(val: string): void {
    this.filterChange.emit({
      definitionId: this.definition().id,
      value: val ? val : null
    });
  }

  onSelectChange(val: string): void {
    this.filterChange.emit({
      definitionId: this.definition().id,
      value: val ? val : null
    });
  }

  onRangeChange(): void {
    this.filterChange.emit({
      definitionId: this.definition().id,
      value: null,
      min: this.minVal ? this.minVal : undefined,
      max: this.maxVal ? this.maxVal : undefined
    });
  }
}
