import { Component, OnInit, signal, computed, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomFieldsService, CustomFieldDefinition, CustomFieldTargetEntity, PresetGroup } from '../../../core/services/custom-fields.service';
import { DefinitionListComponent } from './definition-list/definition-list';
import { DefinitionBuilderComponent } from './definition-builder/definition-builder';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { NotificationService } from '../../../core/ui/notification.service';
import { ConfirmDialogComponent } from '../../../core/ui/confirm-dialog';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-custom-fields-admin',
  standalone: true,
  imports: [CommonModule, DefinitionListComponent, DefinitionBuilderComponent, MatDialogModule, RouterModule, MatIconModule],
  template: `
    <div class="p-8 bg-hex-pattern min-h-screen text-slate-900 dark:text-slate-100">
      <div class="max-w-6xl mx-auto">
        
        <!-- Header -->
        <div class="flex flex-col md:flex-row md:items-end justify-between mb-10 gap-4">
          <div>
            <a 
              routerLink="/admin/settings"
              class="flex items-center gap-1.5 text-xs font-bold text-slate-400 hover:text-ink cursor-pointer mb-3 no-underline transition-colors w-fit"
            >
              <mat-icon class="!text-[14px] !w-[14px] !h-[14px]">arrow_back</mat-icon>
              <span>Back to Settings</span>
            </a>
            <h1 class="text-4xl font-bold text-ink tracking-tight mb-2">Custom Fields</h1>
            <p class="text-ink-light font-medium">Manage custom fields for Requisitions, Applications, and Public Job Postings.</p>
          </div>
          <button
            type="button"
            (click)="openCreateBuilder()"
            class="btn-primary flex items-center gap-2 self-start md:self-auto"
          >
            <mat-icon class="!text-[20px] !w-[20px] !h-[20px]">add</mat-icon>
            Add Field
          </button>
        </div>

        <!-- Main Navigation Tabs -->
        <div class="border-b border-slate-200 dark:border-slate-800 mb-6">
          <nav class="-mb-px flex space-x-8" aria-label="Tabs">
            <button
              type="button"
              (click)="selectedEntity.set('Requisition')"
              [class]="selectedEntity() === 'Requisition' ? 'border-honeycomb-500 text-honeycomb-600 dark:text-honeycomb-400' : 'border-transparent text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-300 hover:border-slate-300'"
              class="whitespace-nowrap py-4 px-1 border-b-2 font-bold text-sm transition-all focus:outline-none"
            >
              Requisition
            </button>
            
            <button
              type="button"
              (click)="selectedEntity.set('Application')"
              [class]="selectedEntity() === 'Application' ? 'border-honeycomb-500 text-honeycomb-600 dark:text-honeycomb-400' : 'border-transparent text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-300 hover:border-slate-300'"
              class="whitespace-nowrap py-4 px-1 border-b-2 font-bold text-sm transition-all focus:outline-none"
            >
              Application
            </button>

            <button
              type="button"
              (click)="selectedEntity.set('JobPosting')"
              [class]="selectedEntity() === 'JobPosting' ? 'border-honeycomb-500 text-honeycomb-600 dark:text-honeycomb-400' : 'border-transparent text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-300 hover:border-slate-300'"
              class="whitespace-nowrap py-4 px-1 border-b-2 font-bold text-sm transition-all focus:outline-none"
            >
              Public Job Posting
            </button>
          </nav>
        </div>

        <!-- Application Sub-Tabs (Stages) -->
        @if (selectedEntity() === 'Application') {
          <div class="bg-slate-100 dark:bg-slate-900 p-1.5 rounded-lg inline-flex space-x-1 mb-6 border border-slate-200 dark:border-slate-800">
            <button
              type="button"
              (click)="selectedScope.set('Application_Global')"
              [class]="selectedScope() === 'Application_Global' ? 'bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 shadow-sm' : 'text-slate-500 dark:text-slate-400 hover:text-slate-800 dark:hover:text-slate-200'"
              class="px-4 py-1.5 rounded-md text-xs font-semibold focus:outline-none transition-all"
            >
              General / Profile
            </button>
            <button
              type="button"
              (click)="selectedScope.set('Application_Applied')"
              [class]="selectedScope() === 'Application_Applied' ? 'bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 shadow-sm' : 'text-slate-500 dark:text-slate-400 hover:text-slate-800 dark:hover:text-slate-200'"
              class="px-4 py-1.5 rounded-md text-xs font-semibold focus:outline-none transition-all"
            >
              Screening (Applied)
            </button>
            <button
              type="button"
              (click)="selectedScope.set('Application_Interview')"
              [class]="selectedScope() === 'Application_Interview' ? 'bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 shadow-sm' : 'text-slate-500 dark:text-slate-400 hover:text-slate-800 dark:hover:text-slate-200'"
              class="px-4 py-1.5 rounded-md text-xs font-semibold focus:outline-none transition-all"
            >
              Interview
            </button>
            <button
              type="button"
              (click)="selectedScope.set('Application_Offer')"
              [class]="selectedScope() === 'Application_Offer' ? 'bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 shadow-sm' : 'text-slate-500 dark:text-slate-400 hover:text-slate-800 dark:hover:text-slate-200'"
              class="px-4 py-1.5 rounded-md text-xs font-semibold focus:outline-none transition-all"
            >
              Offer
            </button>
          </div>
        }

        <!-- Help info for current context -->
        <div class="mb-6 p-4 rounded-lg bg-slate-50 dark:bg-slate-900/50 border border-slate-200 dark:border-slate-800 flex items-start space-x-3">
          <svg class="h-5 w-5 text-honeycomb-500 mt-0.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M11.25 11.25l.041-.02a.75.75 0 111.063.852l-.708 2.836a.75.75 0 001.063.852l.041-.028M12 20.25a8.25 8.25 0 110-16.5 8.25 8.25 0 010 16.5z" />
          </svg>
          <div class="text-xs text-slate-800 dark:text-slate-200 font-medium leading-relaxed">
            @if (resolvedTarget() === 'Requisition') {
              <span>These fields will be displayed in the Requisitions creation/editing form and details.</span>
            } @else if (resolvedTarget() === 'Application_Global') {
              <span>These fields are always available on the candidate's profile, regardless of the application's current stage. They can be exposed for candidates to fill during application if marked as "Candidate Facing".</span>
            } @else if (resolvedTarget() === 'Application_Applied') {
              <span>These fields are displayed in the Screening (Applied) stage. They can be exposed for candidates to fill during application if marked as "Candidate Facing".</span>
            } @else if (resolvedTarget() === 'Application_Interview') {
              <span>These fields are displayed and edited in the Interview stage, typically linked to technical details or interview feedback.</span>
            } @else if (resolvedTarget() === 'Application_Offer') {
              <span>These fields are displayed and filled in the Offer stage, allowing the collection of additional pre-hiring details.</span>
            } @else if (resolvedTarget() === 'JobPosting') {
              <span>Additional fields linked to the Job Posting and displayed on the public job posting page. Note: These are purely informative on the posting; for application questions, configure them under the Application scope with "Candidate Facing" enabled.</span>
            }
          </div>
        </div>

        <!-- Main Content / List -->
        @if (isLoading()) {
          <div class="py-12 flex justify-center items-center">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-honeycomb-500"></div>
          </div>
        } @else {
          <app-definition-list
            [definitions]="definitions()"
            (edit)="openEditBuilder($event)"
            (toggleDisable)="onToggleDisable($event)"
            (delete)="onDelete($event)"
            (reorder)="onReorder($event)"
          ></app-definition-list>
        }

        <!-- Definition Builder slide-over panel -->
        @if (isBuilderOpen()) {
          <app-definition-builder
            [definition]="editingDef()"
            [targetEntity]="resolvedTarget()"
            [availablePresets]="availablePresets()"
            (saved)="onBuilderSaved($event)"
            (cancelled)="closeBuilder()"
          ></app-definition-builder>
        }

      </div>
    </div>
  `
})
export class CustomFieldsAdminComponent implements OnInit {
  selectedEntity = signal<CustomFieldTargetEntity | 'Application'>('Requisition');
  selectedScope  = signal<CustomFieldTargetEntity>('Application_Global');

  resolvedTarget = computed<CustomFieldTargetEntity>(() => {
    const entity = this.selectedEntity();
    if (entity === 'Application') {
      return this.selectedScope();
    }
    return entity as CustomFieldTargetEntity;
  });

  definitions = signal<CustomFieldDefinition[]>([]);
  isLoading = signal(false);
  isBuilderOpen = signal(false);
  editingDef = signal<CustomFieldDefinition | null>(null);
  availablePresets = signal<PresetGroup[]>([]);

  private customFieldsService = inject(CustomFieldsService);
  private notificationService = inject(NotificationService);
  private dialog = inject(MatDialog);

  constructor() {
    effect(() => {
      this.loadDefinitions(this.resolvedTarget());
    });
  }

  ngOnInit(): void {
    this.customFieldsService.getAvailablePresets().subscribe({
      next: (presets) => this.availablePresets.set(presets)
    });
  }

  loadDefinitions(target: CustomFieldTargetEntity): void {
    this.isLoading.set(true);
    this.customFieldsService.getDefinitions(target, { includeDisabled: true }).subscribe({
      next: (defs) => {
        this.definitions.set(defs);
        this.isLoading.set(false);
      },
      error: () => {
        this.notificationService.error('Error loading field definitions.');
        this.isLoading.set(false);
      }
    });
  }

  openCreateBuilder(): void {
    this.editingDef.set(null);
    this.isBuilderOpen.set(true);
  }

  openEditBuilder(def: CustomFieldDefinition): void {
    this.editingDef.set(def);
    this.isBuilderOpen.set(true);
  }

  closeBuilder(): void {
    this.isBuilderOpen.set(false);
    this.editingDef.set(null);
  }

  onBuilderSaved(savedDef: CustomFieldDefinition): void {
    this.closeBuilder();
    this.notificationService.success('Field saved successfully!');
    this.loadDefinitions(this.resolvedTarget());
  }

  onToggleDisable(def: CustomFieldDefinition): void {
    const targetState = !def.isDisabled;
    const request = targetState
      ? this.customFieldsService.disableDefinition(def.id)
      : this.customFieldsService.enableDefinition(def.id);

    request.subscribe({
      next: () => {
        this.notificationService.success(targetState ? 'Field disabled successfully!' : 'Field enabled successfully!');
        this.loadDefinitions(this.resolvedTarget());
      },
      error: () => {
        this.notificationService.error('Error updating field status.');
      }
    });
  }

  onReorder(orderedIds: string[]): void {
    this.customFieldsService.reorderDefinitions(orderedIds).subscribe({
      next: () => {
        this.notificationService.success('Ordering updated successfully!');
        this.loadDefinitions(this.resolvedTarget());
      },
      error: () => {
        this.notificationService.error('Error updating fields ordering.');
        this.loadDefinitions(this.resolvedTarget());
      }
    });
  }

  onDelete(def: CustomFieldDefinition): void {
    if (def.valueCount > 0) {
      // In use: only allow disabling
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: 'Field with data',
          message: `This field has values filled in ${def.valueCount} record(s). Deleting it permanently would orphan these records. Do you want to disable it instead to prevent new entries?`,
          confirmText: 'Disable Field',
          cancelText: 'Cancel',
          isDestructive: false
        }
      });
      dialogRef.afterClosed().subscribe(confirmed => {
        if (confirmed) {
          this.customFieldsService.disableDefinition(def.id).subscribe({
            next: () => {
              this.notificationService.success('Field disabled successfully!');
              this.loadDefinitions(this.resolvedTarget());
            },
            error: () => this.notificationService.error('Error disabling field.')
          });
        }
      });
    } else {
      // Not in use: allow permanent delete
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: 'Delete field',
          message: `Do you want to permanently delete the field "${def.label}"? This action cannot be undone.`,
          confirmText: 'Delete',
          cancelText: 'Cancel',
          isDestructive: true
        }
      });
      dialogRef.afterClosed().subscribe(confirmed => {
        if (confirmed) {
          this.customFieldsService.deleteDefinition(def.id).subscribe({
            next: () => {
              this.notificationService.success('Field deleted successfully!');
              this.loadDefinitions(this.resolvedTarget());
            },
            error: () => this.notificationService.error('Error deleting field.')
          });
        }
      });
    }
  }
}
