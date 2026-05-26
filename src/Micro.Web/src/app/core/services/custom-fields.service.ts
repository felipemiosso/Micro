import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export type CustomFieldTargetEntity =
  | 'Requisition'
  | 'Application_Global'
  | 'Application_Applied'
  | 'Application_Interview'
  | 'Application_Offer'
  | 'JobPosting';

export type CustomFieldType =
  | 'ShortText' | 'LongText' | 'Number' | 'Date' | 'Boolean' | 'SingleChoice';

export interface ValidationOptions {
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  minDate?: string;       // ISO date yyyy-MM-dd
  maxDate?: string;
  presets?: string[];     // OR logic — keys from available-presets
  formatMask?: string;
  choices?: string[];
}

export interface CustomFieldDefinition {
  id: string;
  targetEntity: CustomFieldTargetEntity;
  fieldType: CustomFieldType;
  label: string;
  helpText?: string;
  order: number;
  isRequired: boolean;
  isDisabled: boolean;
  isCandidateFacing: boolean;
  validation?: ValidationOptions;
  valueCount: number;
  isGlobal: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CustomFieldValueDto {
  definitionId: string;
  label: string;
  fieldType: CustomFieldType;
  value?: string;
  isDisabled: boolean;
}

export interface PresetGroup {
  tag: string;
  presets: PresetItem[];
}

export interface PresetItem {
  key: string;
  label: string;
}

export interface CustomFieldValueInput {
  definitionId: string;
  value: string | null;
}

export interface CreateCustomFieldRequest {
  targetEntity: CustomFieldTargetEntity;
  fieldType: CustomFieldType;
  label: string;
  helpText?: string;
  isRequired: boolean;
  isCandidateFacing: boolean;
  validation?: ValidationOptions;
  isGlobal?: boolean;
}

export interface UpdateCustomFieldRequest {
  label: string;
  helpText?: string;
  isRequired: boolean;
  isCandidateFacing: boolean;
  validation?: ValidationOptions;
  isGlobal?: boolean;
}

@Injectable({ providedIn: 'root' })
export class CustomFieldsService {
  private http = inject(HttpClient);

  getDefinitions(
    entity: CustomFieldTargetEntity,
    options: { includeDisabled?: boolean; requisitionId?: string; jobPostingId?: string; applicationId?: string } = {}
  ): Observable<CustomFieldDefinition[]> {
    let params = new HttpParams()
      .set('entity', entity)
      .set('includeDisabled', (options.includeDisabled ?? false).toString());
    if (options.requisitionId) {
      params = params.set('requisitionId', options.requisitionId);
    }
    if (options.jobPostingId) {
      params = params.set('jobPostingId', options.jobPostingId);
    }
    if (options.applicationId) {
      params = params.set('applicationId', options.applicationId);
    }
    return this.http.get<CustomFieldDefinition[]>('/api/custom-fields', { params });
  }

  getAvailablePresets(): Observable<PresetGroup[]> {
    return this.http.get<PresetGroup[]>('/api/custom-fields/available-presets');
  }

  createDefinition(req: CreateCustomFieldRequest): Observable<CustomFieldDefinition> {
    return this.http.post<CustomFieldDefinition>('/api/custom-fields', req);
  }

  updateDefinition(id: string, req: UpdateCustomFieldRequest): Observable<CustomFieldDefinition> {
    return this.http.put<CustomFieldDefinition>(`/api/custom-fields/${id}`, req);
  }

  disableDefinition(id: string): Observable<void> {
    return this.http.patch<void>(`/api/custom-fields/${id}/disable`, {});
  }

  enableDefinition(id: string): Observable<void> {
    return this.http.patch<void>(`/api/custom-fields/${id}/enable`, {});
  }

  reorderDefinitions(orderedIds: string[]): Observable<void> {
    return this.http.put<void>('/api/custom-fields/reorder', { orderedIds });
  }

  deleteDefinition(id: string): Observable<void> {
    return this.http.delete<void>(`/api/custom-fields/${id}`);
  }

  getSelectableDefinitions(targetEntity?: CustomFieldTargetEntity): Observable<CustomFieldDefinition[]> {
    let params = new HttpParams();
    if (targetEntity) {
      params = params.set('targetEntity', targetEntity);
    }
    return this.http.get<CustomFieldDefinition[]>('/api/custom-fields/selectable', { params });
  }

  linkRequisitionField(requisitionId: string, definitionId: string): Observable<void> {
    return this.http.post<void>(`/api/requisitions/${requisitionId}/custom-fields`, {
      customFieldDefinitionId: definitionId
    });
  }

  unlinkRequisitionField(requisitionId: string, definitionId: string): Observable<void> {
    return this.http.delete<void>(`/api/requisitions/${requisitionId}/custom-fields/${definitionId}`);
  }

  linkJobPostingField(jobPostingId: string, definitionId: string): Observable<void> {
    return this.http.post<void>(`/api/jobs/${jobPostingId}/custom-fields`, {
      customFieldDefinitionId: definitionId
    });
  }

  unlinkJobPostingField(jobPostingId: string, definitionId: string): Observable<void> {
    return this.http.delete<void>(`/api/jobs/${jobPostingId}/custom-fields/${definitionId}`);
  }
}
