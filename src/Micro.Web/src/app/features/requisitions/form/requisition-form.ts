import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Observable } from 'rxjs';
import { RequisitionService } from '../requisition.service';
import { NotificationService } from '../../../core/ui/notification.service';
import { AdminService, Department, SalaryBand, CostCenter } from '../../admin/admin.service';

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
  private adminService = inject(AdminService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private notification = inject(NotificationService);

  departments = signal<Department[]>([]);
  salaryBands = signal<SalaryBand[]>([]);
  costCenters = signal<CostCenter[]>([]);

  form = this.fb.group({
    title: ['', [Validators.required]],
    departmentId: ['', [Validators.required]],
    salaryBandId: ['', [Validators.required]],
    costCenterId: ['', [Validators.required]],
    openingsCount: [1, [Validators.required, Validators.min(1)]],
    employmentType: [0, [Validators.required]],
    workplaceType: [0, [Validators.required]],
    location: ['', [Validators.required]],
    jobDescription: ['', [Validators.required]],
    isInternalOnly: [false],
    targetStartDate: [null as string | null]
  });

  isEdit = false;
  requisitionId?: string;

  ngOnInit() {
    this.loadLookups();
    this.requisitionId = this.route.snapshot.paramMap.get('id') ?? undefined;
    if (this.requisitionId) {
      this.isEdit = true;
      this.requisitionService.getById(this.requisitionId).subscribe({
        next: (req) => {
          this.form.patchValue(req);
          if (req.status !== 0) { // Not Draft
            this.form.disable();
          }
        },
        error: (err) => {
          this.notification.error('Failed to load requisition details.');
          this.router.navigate(['/requisitions']);
        }
      });
    }
  }

  loadLookups() {
    this.adminService.getDepartments().subscribe(data => this.departments.set(data));
    this.adminService.getSalaryBands().subscribe(data => this.salaryBands.set(data));
    this.adminService.getCostCenters().subscribe(data => this.costCenters.set(data));
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
        this.router.navigate(['/requisitions']);
      },
      error: (err) => {
        this.notification.error('Failed to save requisition.');
      }
    });
  }
}
