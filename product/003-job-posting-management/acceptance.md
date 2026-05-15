# Job Posting Management - Acceptance Criteria

## Scope
This document covers the lifecycle and public visibility of Job Postings, including their relationship with Requisitions.

## Scenarios

### Admin Lifecycle

#### AC-01: Automatic Creation on Finalization [layer: e2e]
```
Given I am an Admin
And a "Draft" Requisition exists
When I finalize the Requisition
Then a new Job Posting is automatically created with the same Title
And its status is set to "Published"
```

#### AC-02: Edit Job Posting Details [layer: e2e]
```
Given a "Published" Job Posting exists
When I update its public description and requirements
Then the changes are saved and visible to public users
```

#### AC-03: Manual Job Posting Closure [layer: e2e]
```
Given a "Published" Job Posting exists
When I manually close the Job Posting
Then its status changes to "Closed"
And it is no longer visible on the public job board
But the parent Requisition remains "Finalized"
```

#### AC-04: Automatic Closure via Requisition [layer: integration]
```
Given a "Finalized" Requisition has an associated "Published" Job Posting
When I close the Requisition
Then the associated Job Posting is automatically set to "Closed"
```

### Public Experience

#### AC-05: Public Job Board Visibility [layer: e2e]
```
Given multiple Job Postings exist with "Published" and "Closed" statuses
When a public user visits the Job Board
Then they only see the "Published" Job Postings
```

#### AC-06: Public Job Detail Access [layer: e2e]
```
Given a "Published" Job Posting exists
When a public user views the details of that specific job
Then they see the job title, department, description, and requirements
```
