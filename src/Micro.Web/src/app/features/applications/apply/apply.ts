import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators, FormControl, FormGroup } from '@angular/forms';
import { ApplicationService } from '../application.service';
import { JobPostingService, PublicJobDetailResponse } from '../../job-postings/job-posting.service';
import { CustomFieldFormService } from '../../../core/services/custom-field-form.service';
import { CustomFieldInputComponent } from '../../../core/ui/custom-field-input/custom-field-input';

@Component({
  selector: 'app-apply',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, CustomFieldInputComponent],
  templateUrl: './apply.html',
})
export class ApplyComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private applicationService = inject(ApplicationService);
  private jobPostingService = inject(JobPostingService);
  private customFieldFormService = inject(CustomFieldFormService);

  jobId: string | null = null;
  job = signal<PublicJobDetailResponse | null>(null);
  submitting = signal(false);
  errorMessage = signal<string | null>(null);

  customFieldsGroup = signal<FormGroup | null>(null);

  applyForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
    phone: ['', [Validators.maxLength(50)]],
    resume: [null as File | null, [Validators.required]],
  });

  getCustomFieldControl(id: string): FormControl {
    return this.applyForm.get(`customFields.${id}`) as unknown as FormControl;
  }

  ngOnInit() {
    this.jobId = this.route.snapshot.paramMap.get('id');
    if (this.jobId) {
      this.jobPostingService.getJobDetail(this.jobId).subscribe({
        next: (data: PublicJobDetailResponse) => {
          this.job.set(data);
          const fields = data.candidateFacingFields || [];
          const group = this.customFieldFormService.buildFormGroup(fields, []);
          this.customFieldsGroup.set(group);
          (this.applyForm as any).addControl('customFields', group);
        },
        error: (err: any) => {
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

    // Append custom fields
    if (this.customFieldsGroup()) {
      const fields = this.job()?.candidateFacingFields || [];
      fields.forEach(def => {
        const ctrl = this.customFieldsGroup()?.get(def.id);
        if (ctrl && ctrl.value !== null && ctrl.value !== undefined && ctrl.value !== '') {
          formData.append(`customFields[${def.id}]`, ctrl.value);
        }
      });
    }

    this.applicationService.apply(this.jobId, formData).subscribe({
      next: () => {
        this.router.navigate(['/jobs/apply/success']);
      },
      error: (err: any) => {
        this.submitting.set(false);
        if (err.status === 409) {
          this.errorMessage.set('You have already applied for this position.');
        } else if (err.status === 422 && err.error?.errors) {
          this.customFieldFormService.applyServerErrors(err.error.errors, this.customFieldsGroup()!);
          this.errorMessage.set('Please correct the errors in the custom fields.');
        } else {
          this.errorMessage.set('An error occurred while submitting your application. Please try again.');
        }
        console.error(err);
      }
    });
  }
}
