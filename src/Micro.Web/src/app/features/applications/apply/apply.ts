import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApplicationService } from '../application.service';
import { JobPostingService, PublicJobDetailResponse } from '../../job-postings/job-posting.service';

@Component({
  selector: 'app-apply',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './apply.html',
  styleUrls: ['./apply.css'],
})
export class ApplyComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private applicationService = inject(ApplicationService);
  private jobPostingService = inject(JobPostingService);

  jobId: string | null = null;
  job = signal<PublicJobDetailResponse | null>(null);
  submitting = signal(false);
  errorMessage = signal<string | null>(null);

  applyForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
    phone: ['', [Validators.maxLength(50)]],
    resume: [null as File | null, [Validators.required]],
  });

  ngOnInit() {
    this.jobId = this.route.snapshot.paramMap.get('id');
    if (this.jobId) {
      this.jobPostingService.getJobById(this.jobId).subscribe({
        next: (data) => this.job.set(data),
        error: (err) => {
          this.errorMessage.set('Job not found or no longer accepting applications.');
          console.error(err);
        }
      });
    }
  }

  onFileChange(event: any) {
    const file = event.target.files[0];
    if (file) {
      if (file.type !== 'application/pdf') {
        this.applyForm.patchValue({ resume: null });
        this.errorMessage.set('Only PDF files are allowed.');
        return;
      }
      if (file.size > 5 * 1024 * 1024) {
        this.applyForm.patchValue({ resume: null });
        this.errorMessage.set('File too large. Max 5MB allowed.');
        return;
      }
      this.applyForm.patchValue({ resume: file });
      this.errorMessage.set(null);
    }
  }

  onSubmit() {
    if (this.applyForm.invalid || !this.jobId) return;

    this.submitting.set(true);
    this.errorMessage.set(null);

    const formData = new FormData();
    formData.append('name', this.applyForm.value.name!);
    formData.append('email', this.applyForm.value.email!);
    if (this.applyForm.value.phone) {
      formData.append('phone', this.applyForm.value.phone);
    }
    formData.append('resume', this.applyForm.value.resume!);

    this.applicationService.apply(this.jobId, formData).subscribe({
      next: () => {
        this.router.navigate(['/jobs/apply/success']);
      },
      error: (err) => {
        this.submitting.set(false);
        if (err.status === 409) {
          this.errorMessage.set('You have already applied for this position.');
        } else {
          this.errorMessage.set('An error occurred while submitting your application. Please try again.');
        }
        console.error(err);
      }
    });
  }
}
