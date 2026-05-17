# Authentication — Frontend Technical Design

## Overview
Integration with Firebase Authentication SDK to handle user sessions and provide tokens for backend requests.

## Firebase Integration
- **SDK**: `firebase/app`, `firebase/auth`.
- **Initialization**: Firebase is initialized in `AuthService`.
- **State Management**: User state is tracked using the `onAuthStateChanged` listener.

## AuthService Structure
- `user`: `Signal<UserProfile | null>` - Current user profile data from the backend.
- `isAuthenticated`: `Signal<boolean>` - Derived from Firebase auth state.
- `login(credentials)`: Calls Firebase `signInWithEmailAndPassword`.
- `logout()`: Calls Firebase `signOut`.
- `getToken()`: Returns the latest Firebase ID Token stored in localStorage.

## Interceptors
- `authInterceptor`: Automatically attaches the `Authorization: Bearer <token>` header to all outgoing HTTP requests if a token is present in localStorage.

## Route Guards
- `authGuard`: Protects admin routes by checking the `isAuthenticated` signal from `AuthService`.

## Reactive Patterns
- Uses Angular Signals for all reactive data.
- Profile data is automatically fetched from `/api/profile` upon successful Firebase authentication.
- Tokens are automatically refreshed by the Firebase SDK and updated in local state.
