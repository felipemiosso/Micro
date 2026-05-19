import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Role } from '../roles/role.service';

export interface UserResponse {
  id: string;
  email: string;
  fullName: string;
  isInvitePending: boolean;
  inviteSentAt?: string;
  roles: Role[];
}

export interface InviteUserRequest {
  email: string;
  fullName: string;
  roleIds: string[];
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private baseUrl = '/api/users';

  getUsers(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(this.baseUrl);
  }

  inviteUser(request: InviteUserRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.baseUrl}/invite`, request);
  }

  resendInvite(userId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${userId}/resend-invite`, {});
  }

  manageRoles(userId: string, roleIds: string[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${userId}/roles`, { roleIds });
  }

  deleteUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${userId}`);
  }
}
