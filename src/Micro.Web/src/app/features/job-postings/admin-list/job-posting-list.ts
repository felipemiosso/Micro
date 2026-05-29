import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { JobPostingService, JobPosting, JobPostingStatus } from '../job-posting.service';
import { AuthService } from '../../../core/auth/auth';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../core/ui/confirm-dialog';
import { HttpParams } from '@angular/common/http';
import { PaginationComponent } from '../../../core/ui/pagination/pagination.component';

@Component({
  selector: 'app-job-posting-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule, PaginationComponent],
  templateUrl: './job-posting-list.html',
})
export class JobPostingListComponent implements OnInit {
  private jobPostingService = inject(JobPostingService);
  private dialog = inject(MatDialog);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  authService = inject(AuthService);
  
  jobPostings = signal<JobPosting[]>([]);
  Status = JobPostingStatus;

  // Pagination signals
  page = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);
  totalPages = signal(1);

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const pageVal = params['page'] ? Number(params['page']) : 1;
      const pageSizeVal = params['pageSize'] ? Number(params['pageSize']) : 20;
      this.page.set(pageVal);
      this.pageSize.set(pageSizeVal);

      let httpParams = new HttpParams()
        .set('page', pageVal.toString())
        .set('pageSize', pageSizeVal.toString());

      this.loadJobs(httpParams);
    });
  }

  loadJobs(params?: HttpParams) {
    this.jobPostingService.getAllJobs(params).subscribe({
      next: (data) => {
        this.jobPostings.set(data.items);
        this.totalCount.set(data.totalCount);
        this.page.set(data.page);
        this.pageSize.set(data.pageSize);
        this.totalPages.set(data.totalPages);
      },
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
        this.jobPostingService.closeJob(id).subscribe(() => {
          let httpParams = new HttpParams()
            .set('page', this.page().toString())
            .set('pageSize', this.pageSize().toString());
          this.loadJobs(httpParams);
        });
      }
    });
  }

  getStatusLabel(status: JobPostingStatus): string {
    return status === JobPostingStatus.Published ? 'Published' : 'Closed';
  }

  onPageChange(page: number) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page },
      queryParamsHandling: 'merge'
    });
  }

  onPageSizeChange(pageSize: number) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: 1, pageSize },
      queryParamsHandling: 'merge'
    });
  }
}
