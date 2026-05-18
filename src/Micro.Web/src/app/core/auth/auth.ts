import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap, of, catchError, from, filter, take, firstValueFrom } from 'rxjs';
import { initializeApp } from 'firebase/app';
import { getAuth, signInWithEmailAndPassword, signOut, onAuthStateChanged, User, getIdToken, connectAuthEmulator } from 'firebase/auth';

// Firebase configuration - Replace with your own config
const firebaseConfig = {
  apiKey: "AIzaSyAsUYRSb6bz6MdMARNVS7YnEniiYzFGfT8",
  authDomain: "demo-micro-ats.firebaseapp.com",
  projectId: "demo-micro-ats",
  storageBucket: "demo-micro-ats.firebasestorage.app",
  messagingSenderId: "630052761440",
  appId: "1:630052761440:web:84e3d15ecfd95b0cf5f805",
  measurementId: "G-GK96Y1ET8L"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const auth = getAuth(app);

if (window.location.hostname === 'localhost') {
  connectAuthEmulator(auth, 'http://localhost:9099');
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
  private readonly profileUrl = '/api/profile';
  private readonly tokenKey = 'micro_ats_token';
  
  user = signal<UserProfile | null>(null);
  isAuthenticated = signal<boolean>(false);
  isInitialized = signal<boolean>(false); // Track if Firebase auth state was resolved at least once
  private _token = signal<string | null>(localStorage.getItem(this.tokenKey));

  constructor() {
    // Listen for auth state changes
    onAuthStateChanged(auth, async (firebaseUser: User | null) => {
      if (firebaseUser) {
        const token = await getIdToken(firebaseUser);
        localStorage.setItem(this.tokenKey, token);
        this._token.set(token);
        this.isAuthenticated.set(true);
        this.syncProfile().subscribe({
          complete: () => this.isInitialized.set(true)
        });
      } else {
        localStorage.removeItem(this.tokenKey);
        this._token.set(null);
        this.isAuthenticated.set(false);
        this.user.set(null);
        this.isInitialized.set(true);
      }
    });
  }

  login(credentials: any) {
    return from(signInWithEmailAndPassword(auth, credentials.email, credentials.password)).pipe(
      catchError(error => {
        throw error;
      })
    );
  }

  syncProfile() {
    return this.http.post<UserProfile>(this.profileUrl, {}).pipe(
      tap(profile => {
        this.user.set(profile);
      }),
      catchError(err => {
        console.error('Failed to sync profile', err);
        return of(null);
      })
    );
  }

  logout() {
    return from(signOut(auth));
  }

  getToken(): string | null {
    return this._token();
  }
}
