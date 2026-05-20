import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { RequisitionService, Requisition, RequisitionStatus, OpeningStatus } from '../requisition.service';
import { AuthService } from '../../../core/auth/auth';
import { NotificationService } from '../../../core/ui/notification.service';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../core/ui/confirm-dialog';

@Component({
  selector: 'app-requisition-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule],
  templateUrl: './requisition-list.html',
  styleUrls: ['./requisition-list.css'],
})
export class RequisitionListComponent implements OnInit {
  private requisitionService = inject(RequisitionService);
  private notification = inject(NotificationService);
  private dialog = inject(MatDialog);
  authService = inject(AuthService);
  
  requisitions = signal<Requisition[]>([]);
  Status = RequisitionStatus;

  ngOnInit() {
    this.loadRequisitions();
  }

  loadRequisitions() {
    this.requisitionService.getAll().subscribe({
      next: (data) => {
        this.requisitions.set(data);
      },
      error: (err) => {
        console.error('Failed to load requisitions', err);
        this.notification.error('Failed to load requisitions. Please check if you are logged in.');
      }
    });
  }

  finalize(id: string) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Finalize Requisition',
        message: 'Are you sure you want to finalize this requisition? It will become read-only and ready for job posting.',
        confirmText: 'Finalize',
        isDestructive: false
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.requisitionService.finalize(id).subscribe(() => this.loadRequisitions());
      }
    });
  }

  close(id: string) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Close Requisition',
        message: 'Are you sure you want to close this requisition? This action cannot be undone.',
        confirmText: 'Close Requisition',
        isDestructive: true
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.requisitionService.close(id).subscribe(() => this.loadRequisitions());
      }
    });
  }

  getStatusLabel(status: RequisitionStatus): string {
    switch (status) {
      case RequisitionStatus.Draft: return 'Draft';
      case RequisitionStatus.Finalized: return 'Finalized';
      case RequisitionStatus.Closed: return 'Closed';
      default: return 'Unknown';
    }
  }

  getFilledRatio(req: Requisition): string {
    if (req.status === RequisitionStatus.Draft) {
      return `${req.openingsCount} (Draft)`;
    }
    const filled = req.openings ? req.openings.filter(o => o.status === OpeningStatus.Filled).length : 0;
    return `${filled} / ${req.openingsCount}`;
  }
}

