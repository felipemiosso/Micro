import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RequisitionOpening } from '../requisitions/requisition.service';

export enum ApplicationStatus {
  Applied = 'Applied',
  Interview = 'Interview',
  Offer = 'Offer',
  Archive = 'Archive',
}

export enum ArchivalResolution {
  None = 'None',
  Hired = 'Hired',
  Rejected = 'Rejected',
  Declined = 'Declined',
  Withdrawn = 'Withdrawn',
}

export interface Feedback {
  id: string;
  notes: string;
  score: number;
  createdAt: string;
}

export interface InterviewDetails {
  scheduledDate?: string;
  interviewerName?: string;
}

export interface OfferDetails {
  proposedSalary?: number;
  targetStartDate?: string;
  deadline?: string;
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
  requisitionOpeningId?: string;
  openingSequenceNumber?: number;
  requisitionTitle?: string;
  interviewDetails?: InterviewDetails;
  offerDetails?: OfferDetails;
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
  requisitionOpeningId?: string;
}

export interface ApplicationDetail extends Application {
  feedbacks: Feedback[];
  interviewDetails?: InterviewDetails;
  offerDetails?: OfferDetails;
}

@Injectable({
  providedIn: 'root',
})
export class ApplicationService {
  private http = inject(HttpClient);
  private publicApiUrl = '/api/public/jobs';
  private adminApiUrl = '/api/applications';
  private candidateApiUrl = '/api/candidates';

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

  updateStatus(id: string, status: ApplicationStatus, resolution: ArchivalResolution = ArchivalResolution.None, requisitionOpeningId?: string): Observable<void> {
    return this.http.put<void>(`${this.adminApiUrl}/${id}/status`, { status, resolution, requisitionOpeningId });
  }

  updateInterviewDetails(id: string, payload: InterviewDetails): Observable<void> {
    return this.http.put<void>(`${this.adminApiUrl}/${id}/interview-details`, payload);
  }

  updateOfferDetails(id: string, payload: OfferDetails): Observable<void> {
    return this.http.put<void>(`${this.adminApiUrl}/${id}/offer-details`, payload);
  }

  getAvailableOpenings(jobPostingId: string): Observable<RequisitionOpening[]> {
    return this.http.get<RequisitionOpening[]>(`/api/job-postings/${jobPostingId}/available-openings`);
  }

  addFeedback(id: string, notes: string, score: number): Observable<void> {
    return this.http.post<void>(`${this.adminApiUrl}/${id}/feedback`, { notes, score });
  }

  downloadResume(id: string): Observable<Blob> {
    return this.http.get(`${this.adminApiUrl}/${id}/resume`, { responseType: 'blob' });
  }
}

