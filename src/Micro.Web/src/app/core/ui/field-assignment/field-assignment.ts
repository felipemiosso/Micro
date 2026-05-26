import { Component, OnInit, inject, input, signal, computed, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CustomFieldsService, CustomFieldDefinition, CustomFieldTargetEntity } from '../../services/custom-fields.service';
import { NotificationService } from '../notification.service';

@Component({
  selector: 'app-field-assignment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="card p-6 border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 rounded-xl shadow-sm">
      <div class="border-b border-slate-100 dark:border-slate-800 pb-4 mb-4">
        <h3 class="text-base font-bold text-slate-900 dark:text-white uppercase tracking-wider">Custom Fields</h3>
        <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">Manage global and selectable custom fields for this {{ entityType() === 'Requisition' ? 'requisition' : 'job posting' }}.</p>
      </div>

      <!-- Linked Fields List -->
      <div class="space-y-2 mb-6">
        @for (field of activeFields(); track field.id) {
          <div class="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-950 border border-slate-100 dark:border-slate-900 rounded-lg hover:border-slate-200 dark:hover:border-slate-800 transition-colors">
            <div class="flex flex-col gap-0.5">
              <span class="text-sm font-semibold text-slate-800 dark:text-slate-200">{{ field.label }}</span>
              <div class="flex items-center gap-2">
                <span class="text-[10px] text-slate-500 font-mono">{{ field.targetEntity }}</span>
                <span class="text-[10px] text-slate-400 font-mono">({{ field.fieldType }})</span>
              </div>
            </div>
            <div class="flex items-center gap-2">
              @if (field.isGlobal) {
                <span class="badge-status bg-slate-100 text-slate-700 border-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-700">Global</span>
              } @else {
                <span class="badge-status bg-honeycomb-100 text-honeycomb-800 border-honeycomb-200">Selected</span>
                <button (click)="unlinkField(field)" class="text-red-500 hover:text-red-700 p-1.5 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-900 transition-all" title="Unlink Field">
                  <span class="material-icons text-sm flex items-center">link_off</span>
                </button>
              }
            </div>
          </div>
        } @empty {
          <p class="text-xs text-slate-500 italic py-2">No custom fields assigned.</p>
        }
      </div>

      <!-- Link Selector Section -->
      @if (availablePool().length > 0) {
        <div class="flex flex-col gap-2 pt-4 border-t border-slate-100 dark:border-slate-800">
          <label for="fieldSelect" class="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Add Selectable Field from Pool</label>
          <div class="flex gap-2">
            <select id="fieldSelect" [(ngModel)]="selectedFieldId" class="input-honeycomb flex-1 text-xs">
              <option value="">-- Choose custom field template --</option>
              @for (field of availablePool(); track field.id) {
                <option [value]="field.id">{{ field.label }} ({{ field.targetEntity }} - {{ field.fieldType }})</option>
              }
            </select>
            <button (click)="linkField()" [disabled]="!selectedFieldId() || isLoading()" class="btn-primary py-2 px-4 text-xs flex items-center gap-1">
              <span class="material-icons text-xs">link</span> Link
            </button>
          </div>
        </div>
      } @else {
        <p class="text-xs text-slate-400 italic py-2 border-t border-slate-100 dark:border-slate-800">No selectable field templates available in pool.</p>
      }
    </div>
  `
})
export class FieldAssignmentComponent implements OnInit {
  entityId = input.required<string>();
  entityType = input.required<'Requisition' | 'JobPosting'>();
  changed = output<void>();

  private customFieldsService = inject(CustomFieldsService);
  private notification = inject(NotificationService);

  activeFields = signal<CustomFieldDefinition[]>([]);
  selectablePool = signal<CustomFieldDefinition[]>([]);
  selectedFieldId = signal<string>('');
  isLoading = signal<boolean>(false);

  // Compute available pool fields that are not already active
  availablePool = computed(() => {
    const activeIds = new Set(this.activeFields().map(f => f.id));
    return this.selectablePool().filter(f => !activeIds.has(f.id));
  });

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading.set(true);
    const id = this.entityId();

    if (this.entityType() === 'Requisition') {
      forkJoin({
        active: this.customFieldsService.getDefinitions('Requisition', { requisitionId: id }),
        pool: this.customFieldsService.getSelectableDefinitions('Requisition')
      }).subscribe({
        next: ({ active, pool }) => {
          this.activeFields.set(active);
          this.selectablePool.set(pool);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.notification.error('Failed to load custom fields.');
          this.isLoading.set(false);
        }
      });
    } else {
      // JobPosting loads JobPosting + Application scopes
      const scopes: CustomFieldTargetEntity[] = [
        'JobPosting',
        'Application_Global',
        'Application_Applied',
        'Application_Interview',
        'Application_Offer'
      ];

      const activeQueries = scopes.map(scope => 
        this.customFieldsService.getDefinitions(scope, { jobPostingId: id }).pipe(catchError(() => of([])))
      );

      const poolQueries = scopes.map(scope => 
        this.customFieldsService.getSelectableDefinitions(scope).pipe(catchError(() => of([])))
      );

      forkJoin({
        active: forkJoin(activeQueries),
        pool: forkJoin(poolQueries)
      }).subscribe({
        next: ({ active, pool }) => {
          this.activeFields.set(active.flat());
          this.selectablePool.set(pool.flat());
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.notification.error('Failed to load custom fields.');
          this.isLoading.set(false);
        }
      });
    }
  }

  linkField() {
    const fieldId = this.selectedFieldId();
    if (!fieldId) return;

    this.isLoading.set(true);
    const id = this.entityId();

    const linkObs = this.entityType() === 'Requisition'
      ? this.customFieldsService.linkRequisitionField(id, fieldId)
      : this.customFieldsService.linkJobPostingField(id, fieldId);

    linkObs.subscribe({
      next: () => {
        this.notification.success('Custom field linked successfully.');
        this.selectedFieldId.set('');
        this.loadData();
        this.changed.emit();
      },
      error: (err) => {
        console.error(err);
        this.notification.error(err.error?.message || 'Failed to link custom field.');
        this.isLoading.set(false);
      }
    });
  }

  unlinkField(field: CustomFieldDefinition) {
    this.isLoading.set(true);
    const id = this.entityId();

    const unlinkObs = this.entityType() === 'Requisition'
      ? this.customFieldsService.unlinkRequisitionField(id, field.id)
      : this.customFieldsService.unlinkJobPostingField(id, field.id);

    unlinkObs.subscribe({
      next: () => {
        this.notification.success('Custom field unlinked successfully.');
        this.loadData();
        this.changed.emit();
      },
      error: (err) => {
        console.error(err);
        this.notification.error(err.error?.message || 'Failed to unlink custom field.');
        this.isLoading.set(false);
      }
    });
  }
}
