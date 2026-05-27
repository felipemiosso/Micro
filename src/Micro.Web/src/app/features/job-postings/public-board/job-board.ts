import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { JobPostingService, PublicJobResponse } from '../job-posting.service';

@Component({
  selector: 'app-job-board',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './job-board.html',
})
export class JobBoardComponent implements OnInit {
  private jobPostingService = inject(JobPostingService);
  jobs = signal<PublicJobResponse[]>([]);

  ngOnInit() {
    this.jobPostingService.getPublicJobs().subscribe({
      next: (data: PublicJobResponse[]) => this.jobs.set(data),
      error: (err: any) => console.error('Failed to load published jobs', err)
    });
  }
}
