import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ApplicationService, CandidateDetail, ApplicationStatus, ArchivalResolution } from '../application.service';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatDividerModule, MatTooltipModule],
  templateUrl: './application-detail.html',
  styleUrls: ['./application-detail.css'],
})
export class ApplicationDetailComponent implements OnInit {

  private applicationService = inject(ApplicationService);
  private route = inject(ActivatedRoute);

  candidate = signal<CandidateDetail | null>(null);
  loading = signal(true);

  ApplicationStatus = ApplicationStatus;
  ArchivalResolution = ArchivalResolution;

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadCandidate(id);
    }
  }

  loadCandidate(id: string) {
    this.loading.set(true);
    this.applicationService.getCandidate(id).subscribe({
      next: (data) => {
        this.candidate.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load candidate', err);
        this.loading.set(false);
      }
    });
  }

  getStatusLabel(status: ApplicationStatus): string {
    switch (status) {
      case ApplicationStatus.Applied: return 'Applied';
      case ApplicationStatus.Interview: return 'Interview';
      case ApplicationStatus.Offer: return 'Offer';
      case ApplicationStatus.Archive: return 'Archive';
      default: return 'Unknown';
    }
  }

  getResolutionLabel(res: ArchivalResolution): string {
    switch (res) {
      case ArchivalResolution.Hired: return 'Hired';
      case ArchivalResolution.Rejected: return 'Rejected';
      case ArchivalResolution.Declined: return 'Declined';
      case ArchivalResolution.Withdrawn: return 'Withdrawn';
      default: return '';
    }
  }

  downloadResume(appId: string, candidateName: string) {
    this.applicationService.downloadResume(appId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${candidateName}_Resume.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }
}
