# User Profile — Acceptance Criteria

### [layer: e2e] User Menu and Navigation
- **AC-01**: Given I am logged in, the header should display my name and a profile icon/photo.
- **AC-02**: Given I hover over the user menu in the header, a dropdown should appear with "Profile", "Settings", and "Logout" options.
- **AC-03**: Given I click "Profile" in the dropdown, I should be navigated to `/admin/profile`.
- **AC-04**: Given I click "Logout" in the dropdown, I should be logged out and navigated to the login page.

### [layer: e2e] Profile Page Content
- **AC-05**: Given I am on the Profile page, I should see my Full Name and Email Address.
- **AC-06**: Given I am on the Profile page, I should see a profile picture area (placeholder if none exists).

### [layer: integration] Profile API
- **AC-07**: Given a valid JWT token, GET `/api/profile` should return the current user's name, email, and photo URL.
- **AC-08**: Given an invalid or missing token, GET `/api/profile` should return 401 Unauthorized.
