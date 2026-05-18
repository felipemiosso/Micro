# Candidate Profile Separation — Business Specification

### Overview
Current system store candidate info (name, email) inside `Application`. This cause data duplication and hard track candidate across multiple jobs. Change extract candidate data to dedicated `Candidate` table. `Application` link to `Candidate` but keep own `Status` (Hiring Stage). Enable centralized candidate profile while keep job-specific progress independent.

### User Stories
- As **Recruiter**, I want see all applications from same person in one profile, so I understand their history with company.
- As **Recruiter**, I want update candidate phone number once and see change across all their applications, so data stay consistent.
- As **System**, I want auto-link new application to existing candidate by email, so I avoid duplicate profiles.
- As **Candidate**, I want apply to Job A and Job B independently, so my progress in one not affect other.

### Open Questions
- Should we merge existing duplicate candidates by email during migration? (Yes, assume email unique identifier).
- What happen if candidate apply with same email but different name? (System should use latest provided name or allow edit).
