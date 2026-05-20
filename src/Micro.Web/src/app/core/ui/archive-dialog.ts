import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { FormsModule } from '@angular/forms';
import { ArchivalResolution, ApplicationService } from '../../features/applications/application.service';
import { RequisitionOpening } from '../../features/requisitions/requisition.service';

@Component({
  selector: 'app-archive-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatRadioModule, FormsModule],
  template: `
    <h2 mat-dialog-title>Archive Candidate</h2>
    <mat-dialog-content>
      <p>Please select a resolution for archiving this candidate:</p>
      <mat-radio-group [(ngModel)]="selectedResolution" class="flex flex-col gap-2 mt-4">
        <mat-radio-button [value]="Resolutions.Hired">Hired</mat-radio-button>
        <mat-radio-button [value]="Resolutions.Rejected">Rejected</mat-radio-button>
        <mat-radio-button [value]="Resolutions.Declined">Declined</mat-radio-button>
        <mat-radio-button [value]="Resolutions.Withdrawn">Withdrawn</mat-radio-button>
      </mat-radio-group>

      @if (selectedResolution === Resolutions.Hired) {
        <div class="flex flex-col gap-2 mt-4">
          <label class="text-xs font-bold uppercase tracking-wider text-slate-500">Select Available Position/Opening</label>
          <select [(ngModel)]="selectedOpeningId" class="border border-slate-300 rounded-lg p-2 bg-white w-full text-slate-700 focus:outline-none focus:border-primary">
            <option [ngValue]="null">Select an opening...</option>
            @for (op of availableOpenings(); track op.id) {
              <option [value]="op.id">Opening #{{ op.sequenceNumber }} (Target Date: {{ op.targetStartDate ? (op.targetStartDate | date:'MMM d, yyyy') : 'Not Set' }})</option>
            }
          </select>
          @if (availableOpenings().length === 0) {
            <span class="text-xs text-red-500 font-medium">No open positions available. Make sure the requisition is finalized and has open slots.</span>
          }
        </div>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancel</button>
      <button mat-raised-button color="primary" [disabled]="!selectedResolution || (selectedResolution === Resolutions.Hired && !selectedOpeningId)" (click)="submit()">Archive</button>
    </mat-dialog-actions>
  `,
})
export class ArchiveDialogComponent implements OnInit {
  dialogRef = inject(MatDialogRef<ArchiveDialogComponent>);
  data = inject(MAT_DIALOG_DATA) as { jobPostingId: string };
  private applicationService = inject(ApplicationService);

  selectedResolution: ArchivalResolution | null = null;
  selectedOpeningId: string | null = null;
  Resolutions = ArchivalResolution;
  availableOpenings = signal<RequisitionOpening[]>([]);

  ngOnInit() {
    if (this.data?.jobPostingId) {
      this.applicationService.getAvailableOpenings(this.data.jobPostingId).subscribe(openings => {
        this.availableOpenings.set(openings);
      });
    }
  }

  submit() {
    if (this.selectedResolution) {
      this.dialogRef.close({
        resolution: this.selectedResolution,
        requisitionOpeningId: this.selectedResolution === ArchivalResolution.Hired ? this.selectedOpeningId : null
      });
    }
  }
}

