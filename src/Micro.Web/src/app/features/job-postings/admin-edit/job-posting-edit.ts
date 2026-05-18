import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { JobPostingService } from '../job-posting.service';
import { NotificationService } from '../../../core/ui/notification.service';

@Component({
  selector: 'app-job-posting-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './job-posting-edit.html',
  styleUrls: ['./job-posting-edit.css'],
})
export class JobPostingEditComponent implements OnInit {
  private fb = inject(FormBuilder);
  private jobPostingService = inject(JobPostingService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private notification = inject(NotificationService);

  form = this.fb.group({
    title: ['', [Validators.required]],
    description: ['', [Validators.required]],
    requirements: ['', [Validators.required]],
  });

  jobId?: string;

  ngOnInit() {
    this.jobId = this.route.snapshot.paramMap.get('id') ?? undefined;
    if (this.jobId) {
      this.jobPostingService.getAllJobs().subscribe({
        next: (jobs) => {
          const job = jobs.find(j => j.id === this.jobId);
          if (job) {
            this.form.patchValue(job);
          } else {
            this.notification.error('Job posting not found.');
            this.router.navigate(['/admin/jobs']);
          }
        },
        error: (err) => {
          console.error('Failed to load job postings', err);
          this.notification.error('Failed to load job posting details.');
          this.router.navigate(['/admin/jobs']);
        }
      });
    }
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const data = this.form.getRawValue();
    this.jobPostingService.updateJob(this.jobId!, data as any).subscribe({
      next: () => {
        this.notification.success('Job posting updated successfully.');
        this.router.navigate(['/admin/jobs']);
      },
      error: (err) => {
        console.error('Failed to save job posting', err);
        this.notification.error('Failed to save job posting. Please try again.');
      }
    });
  }
}
