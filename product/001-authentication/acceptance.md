# Acceptance Criteria - Authentication

## Scenario: Successful Admin Login
**Given** the Admin is on the login page
**When** they enter valid credentials (email and password)
**And** click the "Login" button
**Then** the system authenticates the user
**And** redirects them to the Dashboard
**And** the session is stored securely

## Scenario: Failed Login - Invalid Credentials
**Given** the Admin is on the login page
**When** they enter an incorrect email or password
**And** click the "Login" button
**Then** the system shows a "Invalid email or password" error message
**And** remains on the login page

## Scenario: Unauthorized Access Protection
**Given** a user is not logged in
**When** they attempt to access an internal URL (e.g., `/requisitions`)
**Then** the system redirects them to the `/login` page

## Scenario: Admin Logout
**Given** the Admin is logged in
**When** they click the "Logout" button
**Then** the session is cleared
**And** the user is redirected to the login page
