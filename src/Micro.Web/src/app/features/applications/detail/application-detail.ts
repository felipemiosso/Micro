import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApplicationService, CandidateDetail, ApplicationStatus, ArchivalResolution } from '../application.service';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { NotificationService } from '../../../core/ui/notification.service';
import { ArchiveDialogComponent } from '../../../core/ui/archive-dialog';

@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, MatIconModule, MatDividerModule, MatTooltipModule, MatDialogModule],
  templateUrl: './application-detail.html',
  styleUrls: ['./application-detail.css'],
})
export class ApplicationDetailComponent implements OnInit {

  private applicationService = inject(ApplicationService);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);
  private notification = inject(NotificationService);

  candidate = signal<CandidateDetail | null>(null);
  loading = signal(true);

  // Form state
  feedbackNotes = signal('');
  feedbackScore = signal(5);

  ApplicationStatus = ApplicationStatus;
  ArchivalResolution = ArchivalResolution;

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadCandidate(id);
    }
  }

  loadCandidate(id: string) {
    this.loading.set(true);
    this.applicationService.getCandidate(id).subscribe({
      next: (data) => {
        this.candidate.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load candidate', err);
        this.loading.set(false);
      }
    });
  }

  updateStatus(appId: string, status: ApplicationStatus) {
    if (status === ApplicationStatus.Archive) {
      const dialogRef = this.dialog.open(ArchiveDialogComponent);
      dialogRef.afterClosed().subscribe(resolution => {
        if (resolution) {
          this.executeStatusUpdate(appId, status, resolution);
        }
      });
    } else {
      this.executeStatusUpdate(appId, status);
    }
  }

  private executeStatusUpdate(appId: string, status: ApplicationStatus, resolution: ArchivalResolution = ArchivalResolution.None) {
    this.applicationService.updateStatus(appId, status, resolution).subscribe({
      next: () => {
        this.notification.success(`Status updated to ${this.getStatusLabel(status)}`);
        const id = this.route.snapshot.paramMap.get('id');
        if (id) this.loadCandidate(id);
      },
      error: () => this.notification.error('Failed to update status.')
    });
  }

  addFeedback(appId: string) {
    if (!this.feedbackNotes().trim()) return;

    this.applicationService.addFeedback(appId, this.feedbackNotes(), this.feedbackScore()).subscribe({
      next: () => {
        this.notification.success('Feedback added successfully');
        this.feedbackNotes.set('');
        this.feedbackScore.set(5);
        const id = this.route.snapshot.paramMap.get('id');
        if (id) this.loadCandidate(id);
      },
      error: () => this.notification.error('Failed to add feedback')
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

  getResolutionLabel(res: ArchivalResolution): string {
    switch (res) {
      case ArchivalResolution.Hired: return 'Hired';
      case ArchivalResolution.Rejected: return 'Rejected';
      case ArchivalResolution.Declined: return 'Declined';
      case ArchivalResolution.Withdrawn: return 'Withdrawn';
      default: return '';
    }
  }

  downloadResume(appId: string, candidateName: string) {
    this.applicationService.downloadResume(appId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${candidateName}_Resume.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }
}
