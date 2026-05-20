import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormArray } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Observable } from 'rxjs';
import { RequisitionService, Requisition, RequisitionStatus, RequisitionOpening, OpeningStatus } from '../requisition.service';
import { NotificationService } from '../../../core/ui/notification.service';
import { AdminService, Department, SalaryBand, CostCenter } from '../../admin/admin.service';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../core/ui/confirm-dialog';

@Component({
  selector: 'app-requisition-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule],
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
  private dialog = inject(MatDialog);

  departments = signal<Department[]>([]);
  salaryBands = signal<SalaryBand[]>([]);
  costCenters = signal<CostCenter[]>([]);
  requisition = signal<Requisition | null>(null);

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
    targetStartDate: [null as string | null],
    openings: this.fb.array([])
  });

  isEdit = false;
  requisitionId?: string;
  Status = RequisitionStatus;
  OpeningStatus = OpeningStatus;

  get openingsFormArray(): FormArray {
    return this.form.get('openings') as FormArray;
  }

  ngOnInit() {
    this.loadLookups();
    this.requisitionId = this.route.snapshot.paramMap.get('id') ?? undefined;

    // Default targetStartDate propagation and openings count synchronization
    this.form.get('openingsCount')?.valueChanges.subscribe(count => {
      if (count && count > 0) {
        this.syncOpeningsFormArray(count);
      }
    });

    this.form.get('targetStartDate')?.valueChanges.subscribe(date => {
      const openingsArray = this.openingsFormArray;
      openingsArray.controls.forEach(ctrl => {
        if (!ctrl.get('targetStartDate')?.dirty) {
          ctrl.patchValue({ targetStartDate: date ? this.formatDateForInput(date) : null });
        }
      });
    });

    if (this.requisitionId) {
      this.isEdit = true;
      this.requisitionService.getById(this.requisitionId).subscribe({
        next: (req) => {
          this.requisition.set(req);
          this.form.patchValue({
            title: req.title,
            departmentId: req.departmentId,
            salaryBandId: req.salaryBandId,
            costCenterId: req.costCenterId,
            openingsCount: req.openingsCount,
            employmentType: req.employmentType,
            workplaceType: req.workplaceType,
            location: req.location,
            jobDescription: req.jobDescription,
            isInternalOnly: req.isInternalOnly,
            targetStartDate: req.targetStartDate ? this.formatDateForInput(req.targetStartDate) : null
          });
          this.syncOpeningsFormArray(req.openingsCount, req.openings);
          if (req.status !== RequisitionStatus.Draft) {
            this.form.disable();
          }
        },
        error: (err) => {
          this.notification.error('Failed to load requisition details.');
          this.router.navigate(['/requisitions']);
        }
      });
    } else {
      this.syncOpeningsFormArray(1);
    }
  }

  loadLookups() {
    this.adminService.getDepartments().subscribe(data => this.departments.set(data));
    this.adminService.getSalaryBands().subscribe(data => this.salaryBands.set(data));
    this.adminService.getCostCenters().subscribe(data => this.costCenters.set(data));
  }

  syncOpeningsFormArray(count: number, initialOpenings?: RequisitionOpening[]) {
    const openingsArray = this.openingsFormArray;
    while (openingsArray.length > count) {
      openingsArray.removeAt(openingsArray.length - 1);
    }
    const mainDate = this.form.get('targetStartDate')?.value;
    while (openingsArray.length < count) {
      const seq = openingsArray.length + 1;
      const initial = initialOpenings?.find(o => o.sequenceNumber === seq);
      openingsArray.push(this.fb.group({
        sequenceNumber: [seq],
        targetStartDate: [initial?.targetStartDate ? this.formatDateForInput(initial.targetStartDate) : (mainDate ? this.formatDateForInput(mainDate) : null)]
      }));
    }
    // If the array already had elements, make sure we populate them if initialOpenings is provided
    if (initialOpenings) {
      openingsArray.controls.forEach((ctrl, idx) => {
        const initial = initialOpenings.find(o => o.sequenceNumber === idx + 1);
        if (initial) {
          ctrl.patchValue({
            targetStartDate: initial.targetStartDate ? this.formatDateForInput(initial.targetStartDate) : null
          });
        }
      });
    }
  }

  formatDateForInput(dateStr?: string | null): string | null {
    if (!dateStr) return null;
    return dateStr.split('T')[0];
  }

  reloadRequisition() {
    if (!this.requisitionId) return;
    this.requisitionService.getById(this.requisitionId).subscribe({
      next: (req) => {
        this.requisition.set(req);
        this.form.patchValue({
          title: req.title,
          departmentId: req.departmentId,
          salaryBandId: req.salaryBandId,
          costCenterId: req.costCenterId,
          openingsCount: req.openingsCount,
          employmentType: req.employmentType,
          workplaceType: req.workplaceType,
          location: req.location,
          jobDescription: req.jobDescription,
          isInternalOnly: req.isInternalOnly,
          targetStartDate: req.targetStartDate ? this.formatDateForInput(req.targetStartDate) : null
        });
        this.syncOpeningsFormArray(req.openingsCount, req.openings);
        if (req.status !== RequisitionStatus.Draft) {
          this.form.disable();
        }
      }
    });
  }

  updateOpeningDate(opening: RequisitionOpening, newDate: string) {
    if (!this.requisitionId) return;
    this.requisitionService.updateOpening(this.requisitionId, opening.id, {
      targetStartDate: newDate,
      status: opening.status
    }).subscribe({
      next: () => {
        this.notification.success(`Opening #${opening.sequenceNumber} target start date updated.`);
        this.reloadRequisition();
      },
      error: () => {
        this.notification.error('Failed to update target start date.');
      }
    });
  }

  cancelOpening(opening: RequisitionOpening) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: `Cancel Opening #${opening.sequenceNumber}`,
        message: `Are you sure you want to cancel opening #${opening.sequenceNumber}? This will reduce the active headcount for this requisition.`,
        confirmText: 'Cancel Opening',
        isDestructive: true
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.requisitionId) {
        this.requisitionService.updateOpening(this.requisitionId, opening.id, {
          targetStartDate: opening.targetStartDate,
          status: OpeningStatus.Cancelled
        }).subscribe({
          next: () => {
            this.notification.success(`Opening #${opening.sequenceNumber} has been cancelled.`);
            this.reloadRequisition();
          },
          error: () => {
            this.notification.error('Failed to cancel opening.');
          }
        });
      }
    });
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const data = this.form.getRawValue();
    
    // Convert empty date strings to null for backend deserialization
    if (data.targetStartDate === '') {
      data.targetStartDate = null;
    }
    if (data.openings) {
      data.openings.forEach((op: any) => {
        if (op.targetStartDate === '') {
          op.targetStartDate = null;
        }
      });
    }

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
