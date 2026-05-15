# Job Posting Management - Business Overview

## Overview
Job Postings represent the public-facing advertisement of a Finalized Requisition. While Requisitions handle the internal preparation and headcount tracking, Job Postings are designed to attract and collect applications from candidates. They have their own lifecycle but remain linked to their parent Requisition.

## User Stories

### Admin Management
- **As an Admin**, I want the system to automatically create and publish a Job Posting when I finalize a Requisition, so that I don't have to manually duplicate data to start the hiring process.
- **As an Admin**, I want to provide a specific public description and requirements for a Job Posting that might differ from internal requisition notes.
- **As an Admin**, I want to see a list of all Job Postings and their current status (Published or Closed).
- **As an Admin**, I want to be able to close a Job Posting manually (e.g., if we have enough candidates) without necessarily closing the internal Requisition.
- **As an Admin**, I expect that closing a Requisition will automatically close the associated Job Posting, ensuring no new applications are received for a closed position.

### Candidate Experience
- **As a Candidate**, I want to view a list of all currently published job openings so that I can see what positions are available.
- **As a Candidate**, I want to view the full details of a specific job posting, including its description and requirements, to decide if I should apply.

## Open Questions
- Should we allow re-publishing a closed Job Posting if the Requisition is still active? (Assumption: Yes, for now).
- Does the Job Posting need to store a separate Title, or should it always reflect the Requisition's Title? (Assumption: Separate Title allowed, defaulting to Requisition Title).
