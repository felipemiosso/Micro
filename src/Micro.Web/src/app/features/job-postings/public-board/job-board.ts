import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { JobPostingService, PublicJobResponse } from '../job-posting.service';
import { HttpParams } from '@angular/common/http';
import { PaginationComponent } from '../../../core/ui/pagination/pagination.component';

@Component({
  selector: 'app-job-board',
  standalone: true,
  imports: [CommonModule, RouterModule, PaginationComponent],
  templateUrl: './job-board.html',
})
export class JobBoardComponent implements OnInit {
  private jobPostingService = inject(JobPostingService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  jobs = signal<PublicJobResponse[]>([]);

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
    this.jobPostingService.getPublicJobs(params).subscribe({
      next: (data) => {
        this.jobs.set(data.items);
        this.totalCount.set(data.totalCount);
        this.page.set(data.page);
        this.pageSize.set(data.pageSize);
        this.totalPages.set(data.totalPages);
      },
      error: (err) => console.error('Failed to load published jobs', err)
    });
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
