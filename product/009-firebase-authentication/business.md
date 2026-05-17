# Firebase Authentication — Business Specification

### Overview
The system has migrated from local ASP.NET Identity to Firebase Authentication. This shift reduces the burden of managing passwords, hashing, and session state on our backend, while providing a foundation for future features like Social Auth (Google, Microsoft) and multi-factor authentication.

### User Stories
- **External Login**: As an admin, I want to authenticate via Firebase so that my credentials are managed by a secure, industry-standard identity provider.
- **Session Persistence**: As an admin, I want to stay logged in across browser refreshes using Firebase's automatic token management.
- **Seamless Profile**: As an admin, I want my profile details to be automatically synced with the application database on my first login.

### Functional Requirements
- Delegate all login/logout operations to the Firebase SDK.
- Use Firebase ID Tokens (JWT) for all backend communication.
- Maintain a local `Users` table in PostgreSQL to store application-specific data (FullName, PhotoUrl) linked by Firebase UID.

### Open Questions
1. Should we support multiple Firebase providers (Google, Email/Password)? (Yes, Firebase makes this easy, but we start with Email/Password).
2. How do we handle initial user creation? (Handled automatically in the backend when a valid Firebase UID hits `/api/profile` for the first time).
