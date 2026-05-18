import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Observable } from 'rxjs';
import { RequisitionService } from '../requisition.service';
import { NotificationService } from '../../../core/ui/notification.service';

@Component({
  selector: 'app-requisition-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './requisition-form.html',
  styleUrls: ['./requisition-form.css'],
})
export class RequisitionFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private requisitionService = inject(RequisitionService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private notification = inject(NotificationService);

  form = this.fb.group({
    title: ['', [Validators.required]],
    department: ['', [Validators.required]],
    openings: [1, [Validators.required, Validators.min(1)]],
  });

  isEdit = false;
  requisitionId?: string;

  ngOnInit() {
    this.requisitionId = this.route.snapshot.paramMap.get('id') ?? undefined;
    if (this.requisitionId) {
      this.isEdit = true;
      this.requisitionService.getById(this.requisitionId).subscribe({
        next: (req) => {
          console.log('Requisition loaded for edit:', req);
          this.form.patchValue(req);
          if (req.status !== 0) { // Not Draft
            this.form.disable();
          }
        },
        error: (err) => {
          console.error('Failed to load requisition', err);
          this.notification.error('Failed to load requisition details. Please try again.');
          this.router.navigate(['/admin/requisitions']);
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
    const obs: Observable<any> = this.isEdit 
      ? this.requisitionService.update(this.requisitionId!, data as any)
      : this.requisitionService.create(data as any);

    obs.subscribe({
      next: () => {
        this.notification.success('Requisition saved successfully.');
        this.router.navigate(['/admin/requisitions']);
      },
      error: (err) => {
        console.error('Failed to save requisition', err);
        this.notification.error('Failed to save requisition. Please try again.');
      }
    });
  }
}
