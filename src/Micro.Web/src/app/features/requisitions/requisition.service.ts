import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export enum RequisitionStatus {
  Draft = 0,
  Finalized = 1,
  Closed = 2,
}

export interface Requisition {
  id: string;
  title: string;
  department: string;
  openings: number;
  status: RequisitionStatus;
  createdBy: string;
  createdAt: string;
  finalizedAt?: string;
  closedAt?: string;
}

export interface CreateRequisitionRequest {
  title: string;
  department: string;
  openings: number;
}

@Injectable({
  providedIn: 'root',
})
export class RequisitionService {
  private http = inject(HttpClient);
  private apiUrl = '/api/requisitions';

  getAll(): Observable<Requisition[]> {
    return this.http.get<Requisition[]>(this.apiUrl);
  }

  getById(id: string): Observable<Requisition> {
    return this.http.get<Requisition>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateRequisitionRequest): Observable<Requisition> {
    return this.http.post<Requisition>(this.apiUrl, request);
  }

  update(id: string, request: CreateRequisitionRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  finalize(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/finalize`, {});
  }

  close(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/close`, {});
  }
}
