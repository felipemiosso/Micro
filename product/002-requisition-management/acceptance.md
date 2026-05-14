# Requisition Management — Acceptance Criteria

## Scope
This document covers the lifecycle of a Requisition: creation, updating, finalizing, and closing, including business rules and validation.

## Prerequisites / Test Setup
- A user with Admin role is logged into the system.

## Scenarios

### Happy Path: Lifecycle

#### AC-01: Create Requisition [layer: e2e]
```
Given I am an Admin
When I create a Requisition with "Software Engineer", "Engineering", and 2 openings
Then a new Requisition is saved with status "Draft"
And I can see it in the Requisition list.
```

#### AC-02: Update Draft Requisition [layer: e2e]
```
Given a "Draft" Requisition exists
When I update the title to "Senior Software Engineer"
Then the change is saved successfully.
```

#### AC-03: Finalize Requisition [layer: e2e]
```
Given a "Draft" Requisition exists
When I click "Finalize"
Then the status changes to "Finalized"
And I can no longer edit the details.
```

#### AC-04: Close Requisition [layer: e2e]
```
Given a "Finalized" Requisition exists
When I click "Close"
Then the status changes to "Closed"
And it is marked as inactive in the list.
```

### Business Rules & Constraints

#### AC-05: Admin Only Access [layer: integration]
```
Given a user without the "Admin" role
When they attempt to access Requisition management pages or APIs
Then the system denies access (e.g., 403 Forbidden or redirect to home)
```

#### AC-06: Draft Only Edits [layer: e2e]
```
Given a "Finalized" requisition exists
When I try to access the Edit page for this requisition
Then I should be redirected or see a read-only view.
```

#### AC-07: Creation Validation [layer: integration]
```
Given I am creating a Requisition
When I provide an empty Title, empty Department, or Openings less than 1
Then the system rejects the request
And provides a specific error message (e.g., "Title is required.")
```
