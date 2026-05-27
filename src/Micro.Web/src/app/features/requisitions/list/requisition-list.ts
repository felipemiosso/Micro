import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { RequisitionService, Requisition, RequisitionStatus, OpeningStatus } from '../requisition.service';
import { AuthService } from '../../../core/auth/auth';
import { NotificationService } from '../../../core/ui/notification.service';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../core/ui/confirm-dialog';
import { CustomFieldsService, CustomFieldDefinition } from '../../../core/services/custom-fields.service';
import { CustomFieldFilterComponent } from '../../../core/ui/custom-field-filter/custom-field-filter';
import { HttpParams } from '@angular/common/http';

@Component({
  selector: 'app-requisition-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule, CustomFieldFilterComponent],
  templateUrl: './requisition-list.html',
})
export class RequisitionListComponent implements OnInit {
  private requisitionService = inject(RequisitionService);
  private notification = inject(NotificationService);
  private dialog = inject(MatDialog);
  private customFieldsService = inject(CustomFieldsService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  authService = inject(AuthService);

  requisitions = signal<Requisition[]>([]);
  Status = RequisitionStatus;

  // Filter signals
  customFieldDefs = signal<CustomFieldDefinition[]>([]);
  showFilters = signal(false);
  activeFilters = signal<Record<string, { value: string | null, min?: string, max?: string }>>({});

  ngOnInit() {
    this.customFieldsService.getDefinitions('Requisition').subscribe({
      next: (defs) => this.customFieldDefs.set(defs)
    });

    this.route.queryParams.subscribe(params => {
      let httpParams = new HttpParams();
      const newActiveFilters: Record<string, any> = {};
      let hasFilters = false;

      for (const key of Object.keys(params)) {
        if (key.startsWith('cfFilter[')) {
          const val = params[key];
          httpParams = httpParams.set(key, val);
          hasFilters = true;

          const rest = key.substring('cfFilter['.length);
          const bracketIdx = rest.indexOf(']');
          if (bracketIdx > 0) {
            const defId = rest.substring(0, bracketIdx);
            const suffix = rest.substring(bracketIdx + 1);
            if (!newActiveFilters[defId]) {
              newActiveFilters[defId] = { value: null };
            }
            if (suffix === '[min]') {
              newActiveFilters[defId].min = val;
            } else if (suffix === '[max]') {
              newActiveFilters[defId].max = val;
            } else if (suffix === '') {
              newActiveFilters[defId].value = val;
            }
          }
        }
      }

      this.activeFilters.set(newActiveFilters);
      if (hasFilters) {
        this.showFilters.set(true);
      }
      this.loadRequisitions(httpParams);
    });
  }

  loadRequisitions(params?: HttpParams) {
    this.requisitionService.getAll(params).subscribe({
      next: (data) => {
        this.requisitions.set(data);
      },
      error: (err) => {
        console.error('Failed to load requisitions', err);
        this.notification.error('Failed to load requisitions. Please check if you are logged in.');
      }
    });
  }

  finalize(id: string) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Finalize Requisition',
        message: 'Are you sure you want to finalize this requisition? It will become read-only and ready for job posting.',
        confirmText: 'Finalize',
        isDestructive: false
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.requisitionService.finalize(id).subscribe(() => this.loadRequisitions());
      }
    });
  }

  close(id: string) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Close Requisition',
        message: 'Are you sure you want to close this requisition? This action cannot be undone.',
        confirmText: 'Close Requisition',
        isDestructive: true
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.requisitionService.close(id).subscribe(() => this.loadRequisitions());
      }
    });
  }

  getStatusLabel(status: RequisitionStatus): string {
    switch (status) {
      case RequisitionStatus.Draft: return 'Draft';
      case RequisitionStatus.Finalized: return 'Finalized';
      case RequisitionStatus.Closed: return 'Closed';
      default: return 'Unknown';
    }
  }

  getFilledRatio(req: Requisition): string {
    if (req.status === RequisitionStatus.Draft) {
      return `${req.openingsCount} (Draft)`;
    }
    const filled = req.openings ? req.openings.filter(o => o.status === OpeningStatus.Filled).length : 0;
    return `${filled} / ${req.openingsCount}`;
  }

  toggleFilters() {
    this.showFilters.set(!this.showFilters());
    if (!this.showFilters()) {
      // Clear filters on hide
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: {}
      });
    }
  }

  onFilterChange(event: { definitionId: string; value: string | null; min?: string; max?: string }) {
    const current = { ...this.activeFilters() };
    if (event.value === null && event.min === undefined && event.max === undefined) {
      delete current[event.definitionId];
    } else {
      current[event.definitionId] = {
        value: event.value,
        min: event.min,
        max: event.max
      };
    }
    this.activeFilters.set(current);

    const queryParams: Record<string, string | null> = {};
    for (const [defId, filter] of Object.entries(current)) {
      if (filter.value !== null) {
        queryParams[`cfFilter[${defId}]`] = filter.value;
      }
      if (filter.min !== undefined) {
        queryParams[`cfFilter[${defId}][min]`] = filter.min;
      }
      if (filter.max !== undefined) {
        queryParams[`cfFilter[${defId}][max]`] = filter.max;
      }
    }

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams
    });
  }
}

