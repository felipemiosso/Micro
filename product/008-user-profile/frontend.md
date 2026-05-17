# User Profile — Frontend Design

## Components

### Profile Page (`/admin/profile`)
- **Standalone Component**: `ProfileComponent`
- **Layout**: card-honeycomb centered on the page.
- **Content**: Large avatar placeholder, Name header, Email detail.

### Header User Dropdown
- Update `App` component (header) to include a user menu.
- **State**: `user` signal in `AuthService` containing profile info.
- **Style**: Honeycomb dropdown, appears on hover.

## Service Updates

### `AuthService`
- Add `user` signal: `user = signal<UserProfile | null>(null)`.
- Add `loadProfile()` method to fetch data from `/api/profile`.
- Update `login()` to call `loadProfile()` upon success.
- Update `checkToken()` logic to also attempt loading the profile if a token exists.

## Routing
- Add path `admin/profile` to `app.routes.ts` (protected by `authGuard`).
