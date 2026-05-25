import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApplicationService, Application, ApplicationStatus } from '../application.service';
import { AuthService } from '../../../core/auth/auth';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CustomFieldsService, CustomFieldDefinition } from '../../../core/services/custom-fields.service';
import { CustomFieldFilterComponent } from '../../../core/ui/custom-field-filter/custom-field-filter';
import { HttpParams } from '@angular/common/http';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-application-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule, CustomFieldFilterComponent],
  templateUrl: './application-list.html',
  styleUrls: ['./application-list.css'],
})
export class ApplicationListComponent implements OnInit {

  private applicationService = inject(ApplicationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private customFieldsService = inject(CustomFieldsService);
  authService = inject(AuthService);

  applications = signal<Application[]>([]);
  jobPostingId = signal<string | null>(null);
  searchQuery = signal('');
  loading = signal(true);

  // Filter signals
  customFieldDefs = signal<CustomFieldDefinition[]>([]);
  showFilters = signal(false);
  activeFilters = signal<Record<string, { value: string | null, min?: string, max?: string }>>({});

  ngOnInit() {
    forkJoin({
      global: this.customFieldsService.getDefinitions('Application_Global'),
      applied: this.customFieldsService.getDefinitions('Application_Applied')
    }).subscribe({
      next: ({ global, applied }) => {
        this.customFieldDefs.set([...global, ...applied]);
      }
    });

    this.route.queryParams.subscribe(params => {
      this.jobPostingId.set(params['jobPostingId'] || null);
      this.searchQuery.set(params['search'] || '');

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
      this.loadApplications(httpParams);
    });
  }

  loadApplications(httpParams?: HttpParams) {
    this.loading.set(true);
    this.applicationService.getApplications(this.jobPostingId() ?? undefined, this.searchQuery() || undefined, httpParams).subscribe({
      next: (data) => {
        this.applications.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load applications', err);
        this.loading.set(false);
      }
    });
  }

  onSearch() {
    const queryParams: Record<string, string | null> = {};
    if (this.jobPostingId()) {
      queryParams['jobPostingId'] = this.jobPostingId();
    }
    if (this.searchQuery()) {
      queryParams['search'] = this.searchQuery();
    }

    for (const [defId, filter] of Object.entries(this.activeFilters())) {
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

  toggleFilters() {
    this.showFilters.set(!this.showFilters());
    if (!this.showFilters()) {
      const queryParams: Record<string, string | null> = {};
      if (this.jobPostingId()) {
        queryParams['jobPostingId'] = this.jobPostingId();
      }
      if (this.searchQuery()) {
        queryParams['search'] = this.searchQuery();
      }
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams
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
    if (this.jobPostingId()) {
      queryParams['jobPostingId'] = this.jobPostingId();
    }
    if (this.searchQuery()) {
      queryParams['search'] = this.searchQuery();
    }

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

  getStatusLabel(status: ApplicationStatus): string {
    switch (status) {
      case ApplicationStatus.Applied: return 'Applied';
      case ApplicationStatus.Interview: return 'Interview';
      case ApplicationStatus.Offer: return 'Offer';
      case ApplicationStatus.Archive: return 'Archive';
      default: return 'Unknown';
    }
  }

  downloadResume(id: string, candidateName: string) {
    this.applicationService.downloadResume(id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${candidateName}_Resume.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      },
      error: (err) => console.error('Failed to download resume', err)
    });
  }
}
