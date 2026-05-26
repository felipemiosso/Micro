import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Department, SalaryBand, CostCenter } from '../admin/admin.service';
import { CustomFieldValueDto } from '../../core/services/custom-fields.service';

export enum RequisitionStatus {
  Draft = 'Draft',
  Finalized = 'Finalized',
  Closed = 'Closed',
}

export enum OpeningStatus {
  Open = 'Open',
  Filled = 'Filled',
  Cancelled = 'Cancelled',
}

export interface RequisitionOpening {
  id: string;
  requisitionId: string;
  sequenceNumber: number;
  targetStartDate?: string;
  candidateId?: string;
  candidate?: {
    id: string;
    fullName: string;
    email: string;
  };
  status: OpeningStatus;
}

export interface Requisition {
  id: string;
  title: string;
  departmentId: string;
  department?: Department;
  salaryBandId: string;
  salaryBand?: SalaryBand;
  costCenterId: string;
  costCenter?: CostCenter;
  openingsCount: number;
  status: RequisitionStatus;
  createdBy: string;
  createdAt: string;
  finalizedAt?: string;
  closedAt?: string;
  location: string;
  jobDescription: string;
  employmentType: string;
  workplaceType: string;
  isInternalOnly: boolean;
  targetStartDate?: string;
  openings?: RequisitionOpening[];
  customFields?: CustomFieldValueDto[];
}

export interface CreateRequisitionOpeningRequest {
  sequenceNumber: number;
  targetStartDate?: string | null;
}

export interface CreateRequisitionRequest {
  title: string;
  departmentId: string;
  salaryBandId: string;
  costCenterId: string;
  openingsCount: number;
  employmentType: string;
  workplaceType: string;
  location: string;
  jobDescription: string;
  isInternalOnly: boolean;
  targetStartDate?: string | null;
  openings?: CreateRequisitionOpeningRequest[];
  customFieldValues?: { definitionId: string; value: string | null }[];
  linkedCustomFieldIds?: string[];
}

@Injectable({
  providedIn: 'root',
  })
export class RequisitionService {
  private http = inject(HttpClient);
  private apiUrl = '/api/requisitions';

  getAll(params?: import('@angular/common/http').HttpParams): Observable<Requisition[]> {
    return this.http.get<Requisition[]>(this.apiUrl, { params });
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

  updateOpening(requisitionId: string, openingId: string, request: { targetStartDate?: string | null; status?: OpeningStatus }): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${requisitionId}/openings/${openingId}`, request);
  }

  finalize(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/finalize`, {});
  }

  close(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/close`, {});
  }
}
