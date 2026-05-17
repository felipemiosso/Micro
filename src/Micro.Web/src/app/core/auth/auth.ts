import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap, of, catchError } from 'rxjs';

interface AuthResponse {
  token: string;
}

export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  photoUrl: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/auth';
  private readonly profileUrl = '/api/profile';
  private readonly tokenKey = 'micro_ats_token';
  
  user = signal<UserProfile | null>(null);
  isAuthenticated = signal<boolean>(this.hasToken());

  constructor() {
    console.log('AuthService initialized. Authenticated:', this.isAuthenticated());
    if (this.isAuthenticated()) {
      this.loadProfile().subscribe();
    }
  }

  login(credentials: any) {
    console.log('Login attempt for:', credentials.email);
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        console.log('Login success, token received');
        localStorage.setItem(this.tokenKey, response.token);
        this.isAuthenticated.set(true);
        this.loadProfile().subscribe();
      })
    );
  }

  loadProfile() {
    console.log('Loading profile from:', this.profileUrl);
    return this.http.get<UserProfile>(this.profileUrl).pipe(
      tap(profile => {
        console.log('Profile loaded successfully:', profile.fullName);
        this.user.set(profile);
      }),
      catchError(err => {
        console.error('Failed to load profile', err);
        if (err.status === 401) {
          console.warn('Unauthorized - logging out');
          this.logout();
        }
        return of(null);
      })
    );
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    this.isAuthenticated.set(false);
    this.user.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(this.tokenKey);
  }
}
