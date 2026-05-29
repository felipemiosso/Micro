import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApplicationService, Application, ApplicationStatus, ArchivalResolution } from '../application.service';
import { JobPostingService, JobPosting } from '../../job-postings/job-posting.service';
import { ApplicationCardComponent } from '../application-card/application-card';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { HttpParams } from '@angular/common/http';

@Component({
  selector: 'app-archived-applications',
  standalone: true,
  imports: [CommonModule, FormsModule, ApplicationCardComponent, RouterLink, MatIconModule],
  templateUrl: './archived-applications.html',
})
export class ArchivedApplicationsComponent implements OnInit {
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
    const params = new HttpParams().set('page', '1').set('pageSize', '100');
    this.jobService.getAllJobs(params).subscribe(data => this.jobPostings.set(data.items));
  }

  loadApplications() {
    const jobId = this.selectedJobId() === '' ? undefined : this.selectedJobId();
    const limitParams = new HttpParams().set('page', '1').set('pageSize', '50');

    this.applicationService.getApplications(jobId, undefined, ApplicationStatus.Archive, ArchivalResolution.Hired, limitParams).subscribe(data => {
      this.hired.set(data.items);
    });

    this.applicationService.getApplications(jobId, undefined, ApplicationStatus.Archive, ArchivalResolution.Rejected, limitParams).subscribe(data => {
      this.rejected.set(data.items);
    });

    this.applicationService.getApplications(jobId, undefined, ApplicationStatus.Archive, ArchivalResolution.Declined, limitParams).subscribe(data => {
      this.declined.set(data.items);
    });

    this.applicationService.getApplications(jobId, undefined, ApplicationStatus.Archive, ArchivalResolution.Withdrawn, limitParams).subscribe(data => {
      this.withdrawn.set(data.items);
    });
  }
}
