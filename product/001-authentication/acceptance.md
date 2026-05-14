# Authentication — Acceptance Criteria

## Scope
This document covers the user authentication flow, including login, logout, and unauthorized access protection.

## Prerequisites / Test Setup
- A user with Admin role exists in the system.
- The system is accessible via a web browser.

## Scenarios

### Authentication

#### AC-01: Successful Admin Login [layer: e2e]
```
Given the Admin is on the login page
When they enter valid credentials (email and password)
And click the "Login" button
Then the system authenticates the user
And redirects them to the Dashboard
And the session is stored securely
```

#### AC-02: Failed Login - Invalid Credentials [layer: e2e]
```
Given the Admin is on the login page
When they enter an incorrect email or password
And click the "Login" button
Then the system shows a "Invalid email or password" error message
And remains on the login page
```

#### AC-03: Unauthorized Access Protection [layer: e2e]
```
Given a user is not logged in
When they attempt to access an internal URL (e.g., `/requisitions`)
Then the system redirects them to the `/login` page
```

#### AC-04: Admin Logout [layer: e2e]
```
Given the Admin is logged in
When they click the "Logout" button
Then the session is cleared
And the user is redirected to the login page
```
