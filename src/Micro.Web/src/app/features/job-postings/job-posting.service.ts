import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export enum JobPostingStatus {
  Published = 0,
  Closed = 1
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
  requisition?: {
    department: string;
  };
}

export interface UpdateJobPostingRequest {
  title: string;
  description: string;
  requirements: string;
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

@Injectable({
  providedIn: 'root'
})
export class JobPostingService {
  private http = inject(HttpClient);
  private publicApiUrl = '/api/jobs';
  private adminApiUrl = '/api/jobs/admin';

  getPublicJobs(): Observable<PublicJobResponse[]> {
    return this.http.get<PublicJobResponse[]>(this.publicApiUrl);
  }

  getJobDetail(id: string): Observable<PublicJobDetailResponse> {
    return this.http.get<PublicJobDetailResponse>(`${this.publicApiUrl}/${id}`);
  }

  getAllJobs(): Observable<JobPosting[]> {
    return this.http.get<JobPosting[]>(this.adminApiUrl);
  }

  updateJob(id: string, request: UpdateJobPostingRequest): Observable<void> {
    return this.http.put<void>(`${this.publicApiUrl}/${id}`, request);
  }

  closeJob(id: string): Observable<void> {
    return this.http.post<void>(`${this.publicApiUrl}/${id}/close`, {});
  }
}
