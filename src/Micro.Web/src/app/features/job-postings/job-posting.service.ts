import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustomFieldDefinition } from '../../core/services/custom-fields.service';

export enum JobPostingStatus {
  Published = 'Published',
  Closed = 'Closed'
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
  candidateFacingFields?: CustomFieldDefinition[];
}

import { PagedResponse } from '../../core/services/pagination.types';

@Injectable({
  providedIn: 'root'
})
export class JobPostingService {
  private http = inject(HttpClient);
  private publicApiUrl = '/api/jobs';
  private adminApiUrl = '/api/jobs/admin';

  getPublicJobs(params?: import('@angular/common/http').HttpParams): Observable<PagedResponse<PublicJobResponse>> {
    return this.http.get<PagedResponse<PublicJobResponse>>(this.publicApiUrl, { params });
  }

  getJobDetail(id: string): Observable<PublicJobDetailResponse> {
    return this.http.get<PublicJobDetailResponse>(`${this.publicApiUrl}/${id}`);
  }

  getAllJobs(params?: import('@angular/common/http').HttpParams): Observable<PagedResponse<JobPosting>> {
    return this.http.get<PagedResponse<JobPosting>>(this.adminApiUrl, { params });
  }

  updateJob(id: string, request: UpdateJobPostingRequest): Observable<void> {
    return this.http.put<void>(`${this.publicApiUrl}/${id}`, request);
  }

  closeJob(id: string): Observable<void> {
    return this.http.post<void>(`${this.publicApiUrl}/${id}/close`, {});
  }
}
