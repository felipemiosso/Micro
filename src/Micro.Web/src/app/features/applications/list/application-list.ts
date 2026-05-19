import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApplicationService, Application, ApplicationStatus } from '../application.service';
import { AuthService } from '../../../core/auth/auth';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-application-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './application-list.html',
  styleUrls: ['./application-list.css'],
})
export class ApplicationListComponent implements OnInit {

  private applicationService = inject(ApplicationService);
  private route = inject(ActivatedRoute);
  authService = inject(AuthService);

  applications = signal<Application[]>([]);
  jobPostingId = signal<string | null>(null);
  searchQuery = signal('');
  loading = signal(true);

  ngOnInit() {
    this.route.queryParamMap.subscribe(params => {
      this.jobPostingId.set(params.get('jobPostingId'));
      this.loadApplications();
    });
  }

  loadApplications() {
    this.loading.set(true);
    this.applicationService.getApplications(this.jobPostingId() ?? undefined, this.searchQuery() || undefined).subscribe({
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
    this.loadApplications();
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
