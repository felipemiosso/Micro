import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AvailableAction {
  resource: string;
  action: string;
  description: string;
  permission: string;
}

export interface Role {
  id: string;
  name: string;
  permissions: string[];
}

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private http = inject(HttpClient);
  private baseUrl = '/api/roles';

  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(this.baseUrl);
  }

  getAvailableActions(): Observable<AvailableAction[]> {
    return this.http.get<AvailableAction[]>(`${this.baseUrl}/available-actions`);
  }

  createRole(name: string, permissions: string[]): Observable<Role> {
    return this.http.post<Role>(this.baseUrl, { name, permissions });
  }
}
