import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { RequisitionService, Requisition, RequisitionStatus } from '../requisition.service';

@Component({
  selector: 'app-requisition-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './requisition-list.html',
  styleUrls: ['./requisition-list.css'],
})
export class RequisitionListComponent implements OnInit {
  private requisitionService = inject(RequisitionService);
  requisitions = signal<Requisition[]>([]);
  Status = RequisitionStatus;

  ngOnInit() {
    this.loadRequisitions();
  }

  loadRequisitions() {
    this.requisitionService.getAll().subscribe({
      next: (data) => {
        console.log('Requisitions loaded:', data);
        this.requisitions.set(data);
      },
      error: (err) => {
        console.error('Failed to load requisitions', err);
        alert('Failed to load requisitions. Please check if you are logged in.');
      }
    });
  }

  finalize(id: string) {
    if (confirm('Are you sure you want to finalize this requisition? It will become read-only.')) {
      this.requisitionService.finalize(id).subscribe(() => this.loadRequisitions());
    }
  }

  close(id: string) {
    if (confirm('Are you sure you want to close this requisition?')) {
      this.requisitionService.close(id).subscribe(() => this.loadRequisitions());
    }
  }

  getStatusLabel(status: RequisitionStatus): string {
    switch (status) {
      case RequisitionStatus.Draft: return 'Draft';
      case RequisitionStatus.Finalized: return 'Finalized';
      case RequisitionStatus.Closed: return 'Closed';
      default: return 'Unknown';
    }
  }
}
