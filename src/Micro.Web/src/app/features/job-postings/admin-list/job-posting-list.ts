import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { JobPostingService, JobPosting, JobPostingStatus } from '../job-posting.service';
import { AuthService } from '../../../core/auth/auth';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../core/ui/confirm-dialog';

@Component({
  selector: 'app-job-posting-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule],
  templateUrl: './job-posting-list.html',
  styleUrls: ['./job-posting-list.css'],
})
export class JobPostingListComponent implements OnInit {
  private jobPostingService = inject(JobPostingService);
  private dialog = inject(MatDialog);
  authService = inject(AuthService);
  
  jobPostings = signal<JobPosting[]>([]);
  Status = JobPostingStatus;

  ngOnInit() {
    this.loadJobs();
  }

  loadJobs() {
    this.jobPostingService.getAllJobs().subscribe({
      next: (data) => this.jobPostings.set(data),
      error: (err) => console.error('Failed to load job postings', err)
    });
  }

  closeJob(id: string) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Close Job Posting',
        message: 'Are you sure you want to close this job posting? Candidates will no longer be able to apply.',
        confirmText: 'Close Job',
        isDestructive: true
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.jobPostingService.closeJob(id).subscribe(() => this.loadJobs());
      }
    });
  }

  getStatusLabel(status: JobPostingStatus): string {
    return status === JobPostingStatus.Published ? 'Published' : 'Closed';
  }
}
