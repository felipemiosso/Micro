# Business Value - Authentication

## Description
Provide a secure way for the system administrator to access the internal dashboard. As a minimalist ATS, only one "Admin" user is supported for now.

## User Persona
- **Admin**: The only authorized user who manages requisitions, jobs, and candidates.

## Core Requirements
1. **Secure Access**: Only authenticated users can access internal routes.
2. **Persistence**: The session should persist (using JWT or similar) so the admin doesn't have to log in on every refresh.
3. **Single Identity**: For the MVP, we use a single hardcoded or initial admin user.

## Business Rules
- Login requires a valid Email and Password.
- Failed attempts should show a generic error message.
- Unauthorized access to internal pages must redirect to the login page.
