import { Component, OnInit, inject, signal, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApplicationService, CandidateDetail, ApplicationStatus, ArchivalResolution, CandidateApplication } from '../application.service';
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
export class ApplicationDetailComponent implements OnInit, AfterViewInit {

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

  ngAfterViewInit() {
    this.route.fragment.subscribe(fragment => {
      if (fragment) {
        // Wait for data to load and view to render
        const checkExist = setInterval(() => {
          const element = document.getElementById(fragment);
          if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'center' });
            element.classList.add('highlight-pulse');
            clearInterval(checkExist);
          }
        }, 100);

        // Safety timeout
        setTimeout(() => clearInterval(checkExist), 3000);
      }
    });
  }

  loadCandidate(id: string) {
    this.loading.set(true);
    this.applicationService.getCandidate(id).subscribe({
      next: (data) => {
        this.candidate.set({
          ...data,
          applications: data.applications.map(app => {
            const interview = { ...(app.interviewDetails || {}) };
            if (interview.scheduledDate) {
              interview.scheduledDate = interview.scheduledDate.substring(0, 10);
            }
            
            const offer = { ...(app.offerDetails || {}) };
            if (offer.targetStartDate) {
              offer.targetStartDate = offer.targetStartDate.substring(0, 10);
            }
            if (offer.deadline) {
              offer.deadline = offer.deadline.substring(0, 10);
            }
            
            return {
              ...app,
              interviewDetails: interview,
              offerDetails: offer
            };
          })
        });
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load candidate', err);
        this.loading.set(false);
      }
    });
  }

  updateStatus(appId: string, status: ApplicationStatus) {
    const app = this.candidate()?.applications.find(a => a.id === appId);
    if (!app) return;

    if (status === ApplicationStatus.Archive) {
      const dialogRef = this.dialog.open(ArchiveDialogComponent, {
        data: { jobPostingId: app.jobPostingId }
      });
      dialogRef.afterClosed().subscribe((res: { resolution: ArchivalResolution, requisitionOpeningId?: string | null } | null) => {
        if (res && res.resolution) {
          this.executeStatusUpdate(appId, status, res.resolution, res.requisitionOpeningId || undefined);
        }
      });
    } else {
      this.executeStatusUpdate(appId, status);
    }
  }

  private executeStatusUpdate(appId: string, status: ApplicationStatus, resolution: ArchivalResolution = ArchivalResolution.None, requisitionOpeningId?: string) {
    this.applicationService.updateStatus(appId, status, resolution, requisitionOpeningId).subscribe({
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

  saveInterviewDetails(app: CandidateApplication) {
    if (!app.interviewDetails) return;
    this.applicationService.updateInterviewDetails(app.id, app.interviewDetails).subscribe({
      next: () => {
        this.notification.success('Interview details updated successfully');
      },
      error: () => this.notification.error('Failed to update interview details')
    });
  }

  saveOfferDetails(app: CandidateApplication) {
    if (!app.offerDetails) return;
    this.applicationService.updateOfferDetails(app.id, app.offerDetails).subscribe({
      next: () => {
        this.notification.success('Offer details updated successfully');
      },
      error: () => this.notification.error('Failed to update offer details')
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

  downloadResume(appId: string | undefined, candidateName: string) {
    if (!appId) return;
    this.applicationService.downloadResume(appId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = `${candidateName}_Resume.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      },
      error: (err) => {
        console.error('Failed to download resume', err);
        this.notification.error('Failed to download resume. Please try again.');
      }
    });
  }
}
