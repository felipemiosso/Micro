import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export enum JobPostingStatus {
  Published = 0,
  Closed = 1,
}

export interface JobPosting {
  id: string;
  requisitionId: string;
  title: string;
  description: string;
  requirements: string;
  status: JobPostingStatus;
  createdAt: string;
  updatedAt?: string;
  closedAt?: string;
  requisition?: any; // To include department from parent
}

export interface PublicJobResponse {
  id: string;
  title: string;
  department: string;
  description: string;
  createdAt: string;
}

export interface PublicJobDetailResponse {
  id: string;
  title: string;
  department: string;
  description: string;
  requirements: string;
  createdAt: string;
}

export interface UpdateJobPostingRequest {
  title: string;
  description: string;
  requirements: string;
}

@Injectable({
  providedIn: 'root',
})
export class JobPostingService {
  private http = inject(HttpClient);
  private publicApiUrl = '/api/jobs';
  private adminApiUrl = '/api/admin/jobs';

  // Public APIs
  getPublishedJobs(): Observable<PublicJobResponse[]> {
    return this.http.get<PublicJobResponse[]>(this.publicApiUrl);
  }

  getJobById(id: string): Observable<PublicJobDetailResponse> {
    return this.http.get<PublicJobDetailResponse>(`${this.publicApiUrl}/${id}`);
  }

  // Admin APIs
  getAllJobs(): Observable<JobPosting[]> {
    return this.http.get<JobPosting[]>(this.adminApiUrl);
  }

  updateJob(id: string, request: UpdateJobPostingRequest): Observable<void> {
    return this.http.put<void>(`${this.adminApiUrl}/${id}`, request);
  }

  closeJob(id: string): Observable<void> {
    return this.http.post<void>(`${this.adminApiUrl}/${id}/close`, {});
  }
}
