import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export enum ApplicationStatus {
  Applied = 0,
  Interview = 1,
  Offer = 2,
  Archive = 3,
}

export enum ArchivalResolution {
  None = 0,
  Hired = 1,
  Rejected = 2,
  Declined = 3,
  Withdrawn = 4
}

export interface Feedback {
  id: string;
  notes: string;
  score: number;
  createdAt: string;
}

export interface Candidate {
  id: string;
  fullName: string;
  email: string;
  phone?: string;
  createdAt: string;
}

export interface CandidateApplication {
  id: string;
  jobPostingId: string;
  jobTitle: string;
  status: ApplicationStatus;
  archivalResolution: ArchivalResolution;
  appliedAt: string;
  feedbacks: Feedback[];
}

export interface CandidateDetail extends Candidate {
  applications: CandidateApplication[];
}

export interface Application {
  id: string;
  jobPostingId: string;
  jobTitle: string;
  candidateId: string;
  candidateName: string; 
  candidateEmail: string;
  candidatePhone?: string;
  status: ApplicationStatus;
  archivalResolution: ArchivalResolution;
  appliedAt: string;
}

export interface ApplicationDetail extends Application {
  feedbacks: Feedback[];
}

@Injectable({
  providedIn: 'root',
})
export class ApplicationService {
  private http = inject(HttpClient);
  private publicApiUrl = '/api/public/jobs';
  private adminApiUrl = '/api/admin/applications';
  private candidateApiUrl = '/api/admin/candidates';

  // Public APIs
  apply(jobId: string, formData: FormData): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.publicApiUrl}/${jobId}/apply`, formData);
  }

  // Admin APIs
  getApplications(jobPostingId?: string, search?: string): Observable<Application[]> {
    let url = this.adminApiUrl;
    const params: string[] = [];
    if (jobPostingId) params.push(`jobPostingId=${jobPostingId}`);
    if (search) params.push(`search=${encodeURIComponent(search)}`);
    if (params.length > 0) url += `?${params.join('&')}`;
    
    return this.http.get<Application[]>(url);
  }

  getApplication(id: string): Observable<ApplicationDetail> {
    return this.http.get<ApplicationDetail>(`${this.adminApiUrl}/${id}`);
  }

  // Candidate Profile
  getCandidate(id: string): Observable<CandidateDetail> {
    return this.http.get<CandidateDetail>(`${this.candidateApiUrl}/${id}`);
  }

  updateStatus(id: string, status: ApplicationStatus, resolution: ArchivalResolution = ArchivalResolution.None): Observable<void> {
    return this.http.put<void>(`${this.adminApiUrl}/${id}/status`, { status, resolution });
  }

  addFeedback(id: string, notes: string, score: number): Observable<void> {
    return this.http.post<void>(`${this.adminApiUrl}/${id}/feedback`, { notes, score });
  }

  downloadResume(id: string): Observable<Blob> {
    return this.http.get(`${this.adminApiUrl}/${id}/resume`, { responseType: 'blob' });
  }
}
