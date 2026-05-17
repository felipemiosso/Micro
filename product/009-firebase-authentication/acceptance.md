# Firebase Authentication — Acceptance Criteria

## Scope
Verification of the Firebase integration, token validation, and local user synchronization.

## Scenarios

### Auth Integration

#### AC-01: Firebase Login Flow [layer: e2e]
```
Given the user is on the login page
When they enter their Firebase credentials
And click "Login"
Then the Firebase SDK authenticates the user
And the application receives a JWT ID Token
And the user is redirected to the Dashboard
```

#### AC-02: Backend Token Validation [layer: integration]
```
Given an HTTP request to a protected endpoint (e.g., GET /api/requisitions)
And a valid Firebase ID Token in the Authorization header
When the request is processed by the backend
Then the JwtBearer middleware validates the token against Firebase Authority
And the request is allowed to proceed
```

#### AC-03: User Profile Synchronization [layer: integration]
```
Given a user just authenticated via Firebase for the first time
When they request their profile via GET /api/profile
Then the backend identifies the new Firebase UID
And creates a corresponding record in the local Users table
And returns the user profile data
```

#### AC-04: Firebase Logout [layer: e2e]
```
Given an authenticated user
When they click "Logout"
Then the Firebase SDK signs out the user
And the local token is cleared from storage
And the user is redirected to the login page
```
