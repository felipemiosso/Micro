import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export enum ApplicationStatus {
  Applied = 0,
  Interview = 1,
  Offer = 2,
  Archive = 3,
}

export interface Application {
  id: string;
  jobPostingId: string;
  jobTitle: string;
  candidateName: string;
  candidateEmail: string;
  candidatePhone?: string;
  status: ApplicationStatus;
  appliedAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class ApplicationService {
  private http = inject(HttpClient);
  private publicApiUrl = '/api/public/jobs';
  private adminApiUrl = '/api/admin/applications';

  // Public APIs
  apply(jobId: string, formData: FormData): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.publicApiUrl}/${jobId}/apply`, formData);
  }

  // Admin APIs
  getApplications(jobPostingId?: string): Observable<Application[]> {
    const url = jobPostingId ? `${this.adminApiUrl}?jobPostingId=${jobPostingId}` : this.adminApiUrl;
    return this.http.get<Application[]>(url);
  }

  downloadResume(id: string): Observable<Blob> {
    return this.http.get(`${this.adminApiUrl}/${id}/resume`, { responseType: 'blob' });
  }
}
