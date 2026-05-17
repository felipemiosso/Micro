import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap, of, catchError, from } from 'rxjs';
import { initializeApp } from 'firebase/app';
import { getAuth, signInWithEmailAndPassword, signOut, onAuthStateChanged, User, getIdToken } from 'firebase/auth';

// Firebase configuration - Replace with your own config
const firebaseConfig = {
  apiKey: "AIzaSyAsUYRSb6bz6MdMARNVS7YnEniiYzFGfT8",
  authDomain: "microats-89345.firebaseapp.com",
  projectId: "microats-89345",
  storageBucket: "microats-89345.firebasestorage.app",
  messagingSenderId: "630052761440",
  appId: "1:630052761440:web:84e3d15ecfd95b0cf5f805",
  measurementId: "G-GK96Y1ET8L"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const auth = getAuth(app);

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
  private _token = signal<string | null>(localStorage.getItem(this.tokenKey));

  constructor() {
    console.log('AuthService initialized');
    
    // Listen for auth state changes
    onAuthStateChanged(auth, async (firebaseUser: User | null) => {
      console.log('Firebase auth state changed:', firebaseUser?.email);
      
      if (firebaseUser) {
        const token = await getIdToken(firebaseUser);
        localStorage.setItem(this.tokenKey, token);
        this._token.set(token);
        this.isAuthenticated.set(true);
        this.syncProfile().subscribe();
      } else {
        localStorage.removeItem(this.tokenKey);
        this._token.set(null);
        this.isAuthenticated.set(false);
        this.user.set(null);
      }
    });
  }

  login(credentials: any) {
    console.log('Login attempt for:', credentials.email);
    return from(signInWithEmailAndPassword(auth, credentials.email, credentials.password)).pipe(
      tap(() => console.log('Firebase login success')),
      catchError(error => {
        console.error('Firebase login failed', error);
        throw error;
      })
    );
  }

  syncProfile() {
    console.log('Syncing profile with backend...');
    return this.http.post<UserProfile>(this.profileUrl, {}).pipe(
      tap(profile => {
        console.log('Profile synced successfully:', profile.fullName);
        this.user.set(profile);
      }),
      catchError(err => {
        console.error('Failed to sync profile', err);
        return of(null);
      })
    );
  }

  logout() {
    console.log('Logging out');
    return from(signOut(auth));
  }

  getToken(): string | null {
    return this._token();
  }
}
