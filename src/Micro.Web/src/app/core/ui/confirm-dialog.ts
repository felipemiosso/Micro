import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  isDestructive?: boolean;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="p-6 max-w-md">
      <div class="flex items-center gap-3 mb-4">
        <div 
          [class]="data.isDestructive ? 'bg-red-100 text-red-600' : 'bg-honeycomb-100 text-honeycomb-600'"
          class="w-10 h-10 rounded-full flex items-center justify-center shrink-0">
          <mat-icon>{{ data.isDestructive ? 'warning' : 'help_outline' }}</mat-icon>
        </div>
        <h2 class="text-xl font-bold text-ink m-0">{{ data.title }}</h2>
      </div>
      
      <p class="text-slate-600 mb-8 leading-relaxed">
        {{ data.message }}
      </p>

      <div class="flex justify-end gap-3">
        <button 
          mat-button 
          (click)="dialogRef.close(false)"
          class="!rounded-lg !px-6 !font-bold !text-slate-400">
          {{ data.cancelText || 'Cancel' }}
        </button>
        <button 
          [class]="data.isDestructive ? '!bg-red-500 hover:!bg-red-600' : 'btn-primary'"
          class="!rounded-lg !px-8 !font-bold !shadow-none"
          (click)="dialogRef.close(true)">
          {{ data.confirmText || 'Confirm' }}
        </button>
      </div>
    </div>
  `
})
export class ConfirmDialogComponent {
  dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
  data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
}
