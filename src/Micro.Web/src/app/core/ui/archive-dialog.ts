import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { FormsModule } from '@angular/forms';
import { ArchivalResolution } from '../../features/applications/application.service';

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
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancel</button>
      <button mat-raised-button color="primary" [disabled]="!selectedResolution" (click)="submit()">Archive</button>
    </mat-dialog-actions>
  `,
})
export class ArchiveDialogComponent {
  dialogRef = inject(MatDialogRef<ArchiveDialogComponent>);
  selectedResolution: ArchivalResolution | null = null;
  Resolutions = ArchivalResolution;

  submit() {
    if (this.selectedResolution) {
      this.dialogRef.close(this.selectedResolution);
    }
  }
}
