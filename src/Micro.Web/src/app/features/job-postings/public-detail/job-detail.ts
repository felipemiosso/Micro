import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { JobPostingService, PublicJobDetailResponse } from '../job-posting.service';

@Component({
  selector: 'app-job-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './job-detail.html',
  styleUrls: ['./job-detail.css'],
})
export class JobDetailComponent implements OnInit {
  private jobPostingService = inject(JobPostingService);
  private route = inject(ActivatedRoute);
  job = signal<PublicJobDetailResponse | null>(null);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.jobPostingService.getJobDetail(id).subscribe({
        next: (data: PublicJobDetailResponse) => this.job.set(data),
        error: (err: any) => console.error('Failed to load job details', err)
      });
    }
  }
}
