import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApplicationService, Application, ApplicationStatus, ArchivalResolution, ApplicationDetail } from '../application.service';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './application-detail.html',
  styleUrls: ['./application-detail.css'],
})
export class ApplicationDetailComponent implements OnInit {

  private applicationService = inject(ApplicationService);
  private route = inject(ActivatedRoute);

  application = signal<ApplicationDetail | null>(null);
  loading = signal(true);
  
  // Feedback form
  newNotes = signal('');
  newScore = signal(5);
  submittingFeedback = signal(false);

  // Status management
  showArchivalModal = signal(false);
  selectedResolution = signal<ArchivalResolution>(ArchivalResolution.Rejected);

  ApplicationStatus = ApplicationStatus;
  ArchivalResolution = ArchivalResolution;

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadApplication(id);
    }
  }

  loadApplication(id: string) {
    this.loading.set(true);
    this.applicationService.getApplication(id).subscribe({
      next: (data) => {
        this.application.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load application', err);
        this.loading.set(false);
      }
    });
  }

  updateStatus(status: ApplicationStatus) {
    if (status === ApplicationStatus.Archive) {
      this.showArchivalModal.set(true);
      return;
    }

    const id = this.application()?.id;
    if (id) {
      this.applicationService.updateStatus(id, status).subscribe(() => {
        this.loadApplication(id);
      });
    }
  }

  confirmArchive() {
    const id = this.application()?.id;
    if (id) {
      this.applicationService.updateStatus(id, ApplicationStatus.Archive, this.selectedResolution()).subscribe(() => {
        this.showArchivalModal.set(false);
        this.loadApplication(id);
      });
    }
  }

  submitFeedback() {
    const id = this.application()?.id;
    if (id && this.newNotes().trim()) {
      this.submittingFeedback.set(true);
      this.applicationService.addFeedback(id, this.newNotes(), this.newScore()).subscribe({
        next: () => {
          this.newNotes.set('');
          this.newScore.set(5);
          this.submittingFeedback.set(false);
          this.loadApplication(id);
        },
        error: () => this.submittingFeedback.set(false)
      });
    }
  }

  downloadResume() {
    const app = this.application();
    if (app) {
      this.applicationService.downloadResume(app.id).subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `${app.candidateName}_Resume.pdf`;
          a.click();
          window.URL.revokeObjectURL(url);
        }
      });
    }
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

  getResolutionLabel(res: ArchivalResolution): string {
    switch (res) {
      case ArchivalResolution.Hired: return 'Hired';
      case ArchivalResolution.Rejected: return 'Rejected';
      case ArchivalResolution.Declined: return 'Declined';
      case ArchivalResolution.Withdrawn: return 'Withdrawn';
      default: return '';
    }
  }
}
