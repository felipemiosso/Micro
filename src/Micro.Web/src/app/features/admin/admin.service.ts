import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Department {
  id: string;
  name: string;
  isActive: boolean;
}

export interface SalaryBand {
  id: string;
  name: string;
  minAmount: number;
  maxAmount: number;
  currency: string;
}

export interface CostCenter {
  id: string;
  code: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private baseUrl = '/api/admin';

  // Departments
  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.baseUrl}/departments`);
  }
  createDepartment(dept: Partial<Department>): Observable<Department> {
    return this.http.post<Department>(`${this.baseUrl}/departments`, dept);
  }
  updateDepartment(id: string, dept: Partial<Department>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/departments/${id}`, dept);
  }

  // Salary Bands
  getSalaryBands(): Observable<SalaryBand[]> {
    return this.http.get<SalaryBand[]>(`${this.baseUrl}/salary-bands`);
  }
  createSalaryBand(band: Partial<SalaryBand>): Observable<SalaryBand> {
    return this.http.post<SalaryBand>(`${this.baseUrl}/salary-bands`, band);
  }
  updateSalaryBand(id: string, band: Partial<SalaryBand>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/salary-bands/${id}`, band);
  }

  // Cost Centers
  getCostCenters(): Observable<CostCenter[]> {
    return this.http.get<CostCenter[]>(`${this.baseUrl}/cost-centers`);
  }
  createCostCenter(cc: Partial<CostCenter>): Observable<CostCenter> {
    return this.http.post<CostCenter>(`${this.baseUrl}/cost-centers`, cc);
  }
  updateCostCenter(id: string, cc: Partial<CostCenter>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/cost-centers/${id}`, cc);
  }
}
