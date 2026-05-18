import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { ApplicationService, Application, ApplicationStatus, ArchivalResolution } from '../application.service';
import { JobPostingService, JobPosting } from '../../job-postings/job-posting.service';
import { ApplicationCardComponent } from '../application-card/application-card';
import { RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { NotificationService } from '../../../core/ui/notification.service';
import { ArchiveDialogComponent } from '../../../core/ui/archive-dialog';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-applications-board',
  standalone: true,
  imports: [CommonModule, FormsModule, DragDropModule, ApplicationCardComponent, RouterLink, MatIconModule],
  templateUrl: './applications-board.html',
  styleUrl: './applications-board.css',
})
export class ApplicationsBoardComponent implements OnInit {
  private applicationService = inject(ApplicationService);
  private jobService = inject(JobPostingService);
  private dialog = inject(MatDialog);
  private notification = inject(NotificationService);

  jobPostings = signal<JobPosting[]>([]);
  selectedJobId = signal<string>('');

  applied = signal<Application[]>([]);
  interview = signal<Application[]>([]);
  offer = signal<Application[]>([]);

  ngOnInit() {
    this.loadJobs();
    this.loadApplications();
  }

  loadJobs() {
    this.jobService.getAllJobs().subscribe(jobs => this.jobPostings.set(jobs));
  }

  loadApplications() {
    const jobId = this.selectedJobId() === '' ? undefined : this.selectedJobId();
    this.applicationService.getApplications(jobId).subscribe(apps => {
      const activeApps = apps.filter(a => a.status !== ApplicationStatus.Archive);
      this.applied.set(activeApps.filter(a => a.status === ApplicationStatus.Applied));
      this.interview.set(activeApps.filter(a => a.status === ApplicationStatus.Interview));
      this.offer.set(activeApps.filter(a => a.status === ApplicationStatus.Offer));
    });
  }

  onDrop(event: CdkDragDrop<Application[]>) {
    if (event.previousContainer === event.container) {
      const list = [...event.container.data];
      moveItemInArray(list, event.previousIndex, event.currentIndex);
      this.setListByContainerId(event.container.id, list);
    } else {
      const item = event.previousContainer.data[event.previousIndex];
      const newStatus = this.getStatusFromContainerId(event.container.id);

      if (!this.isMoveAllowed(item.status, newStatus)) {
        this.notification.error('Invalid move. Candidates must progress sequentially: Applied -> Interview -> Offer');
        return;
      }

      const prevList = [...event.previousContainer.data];
      const currList = [...event.container.data];
      transferArrayItem(prevList, currList, event.previousIndex, event.currentIndex);
      
      this.setListByContainerId(event.previousContainer.id, prevList);
      this.setListByContainerId(event.container.id, currList);

      this.applicationService.updateStatus(item.id, newStatus).subscribe({
        next: () => {
          item.status = newStatus;
          this.notification.success(`Candidate moved to ${this.getStatusLabel(newStatus)}`);
        },
        error: () => {
          this.notification.error('Failed to move candidate.');
          this.loadApplications();
        },
      });
    }
  }

  private setListByContainerId(id: string, data: Application[]) {
    if (id === 'appliedList') this.applied.set(data);
    else if (id === 'interviewList') this.interview.set(data);
    else if (id === 'offerList') this.offer.set(data);
  }

  private getStatusFromContainerId(id: string): ApplicationStatus {
    switch (id) {
      case 'appliedList': return ApplicationStatus.Applied;
      case 'interviewList': return ApplicationStatus.Interview;
      case 'offerList': return ApplicationStatus.Offer;
      default: return ApplicationStatus.Applied;
    }
  }

  private getStatusLabel(status: ApplicationStatus): string {
    switch (status) {
      case ApplicationStatus.Applied: return 'Applied';
      case ApplicationStatus.Interview: return 'Interview';
      case ApplicationStatus.Offer: return 'Offer';
      default: return 'Archive';
    }
  }

  private isMoveAllowed(current: ApplicationStatus, next: ApplicationStatus): boolean {
    if (current === ApplicationStatus.Applied && next === ApplicationStatus.Interview) return true;
    if (current === ApplicationStatus.Interview && next === ApplicationStatus.Offer) return true;
    return false;
  }

  onArchiveDrop(event: CdkDragDrop<Application[]>) {
    const item = event.previousContainer.data[event.previousIndex];
    const dialogRef = this.dialog.open(ArchiveDialogComponent);

    dialogRef.afterClosed().subscribe(resolution => {
      if (resolution) {
        this.applicationService.updateStatus(item.id, ApplicationStatus.Archive, resolution).subscribe({
          next: () => {
            const list = [...event.previousContainer.data];
            list.splice(event.previousIndex, 1);
            this.setListByContainerId(event.previousContainer.id, list);
            this.notification.success('Candidate archived successfully.');
          },
          error: () => this.notification.error('Failed to archive candidate.')
        });
      }
    });
  }
}
