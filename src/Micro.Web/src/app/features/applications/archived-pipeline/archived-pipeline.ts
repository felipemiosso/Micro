import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApplicationService, Application, ApplicationStatus, ArchivalResolution } from '../application.service';
import { JobPostingService, JobPosting } from '../../job-postings/job-posting.service';
import { ApplicationCardComponent } from '../application-card/application-card';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-archived-pipeline',
  standalone: true,
  imports: [CommonModule, FormsModule, ApplicationCardComponent, RouterLink],
  templateUrl: './archived-pipeline.html',
  styleUrl: './archived-pipeline.css',
})
export class ArchivedPipelineComponent implements OnInit {
  private applicationService = inject(ApplicationService);
  private jobService = inject(JobPostingService);

  jobPostings = signal<JobPosting[]>([]);
  selectedJobId = signal<string>('');

  hired = signal<Application[]>([]);
  rejected = signal<Application[]>([]);
  declined = signal<Application[]>([]);
  withdrawn = signal<Application[]>([]);

  ngOnInit() {
    this.loadJobs();
    this.loadApplications();
  }

  loadJobs() {
    this.jobService.getAllJobs().subscribe(jobs => this.jobPostings.set(jobs));
  }

  loadApplications() {
    const jobId = this.selectedJobId() === '' ? undefined : this.selectedJobId();
    this.applicationService.getApplications(jobId).subscribe(apps => {
      const archivedApps = apps.filter(a => a.status === ApplicationStatus.Archive);
      this.hired.set(archivedApps.filter(a => a.archivalResolution === ArchivalResolution.Hired));
      this.rejected.set(archivedApps.filter(a => a.archivalResolution === ArchivalResolution.Rejected));
      this.declined.set(archivedApps.filter(a => a.archivalResolution === ArchivalResolution.Declined));
      this.withdrawn.set(archivedApps.filter(a => a.archivalResolution === ArchivalResolution.Withdrawn));
    });
  }
}
