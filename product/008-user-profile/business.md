# User Profile — Business Specification

### Overview
The User Profile module allows authenticated users (HR professionals and Recruiters) to manage their personal information and view their activity dashboard. This feature serves as the personal "home base" for the user, providing a centralized location for settings and personal data.

### User Stories
- **View Profile**: As an authenticated user, I want to see my personal information (Name, Email, Photo) so I can verify my identity.
- **Header Dropdown**: As an authenticated user, I want to access my profile and logout options from a user-specific dropdown in the header for quick navigation.
- **Aesthetic Integration**: As a user, I want the profile area to match the professional Honeycomb design system.

### Functional Requirements
- Display user's full name and email on the profile page.
- Display a placeholder or profile picture for the user.
- Add a user menu to the header (Top-Right) that shows the user's name and photo.
- The user menu must open a dropdown on hover with "Settings" (future) and "Logout" options.

### Open Questions
1. Should we allow users to upload their own photo in this first version? (Default: No, use initials or a placeholder for now).
2. Is "Settings" just a placeholder for now? (Yes).
