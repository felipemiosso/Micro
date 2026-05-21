# 017: Stage-Specific Application Data

## Overview
Currently, the application lifecycle supports recording "Feedback" (notes and a score) at any stage. To better support the hiring process, applications must capture dedicated, structured data specific to their current stage. For example, the Offer stage requires capturing formal proposal details like salary and target start date, while the Interview stage might require tracking interviewer details alongside feedback. 

## User Stories
- As a hiring manager, I want to record specific details during the Interview stage (such as Interviewer Name and Interview Date) alongside my assessment feedback.
- As a recruiter, I want to record formal Offer details (Proposed Salary, Target Start Date, Offer Deadline) when a candidate is moved to the Offer stage, so I can track the negotiation process.
- As an administrator, I want to see stage-specific data clearly separated on the Candidate Profile depending on what stage the application is currently in or has passed through.

## Open Questions
1. Should "Interview" data support multiple rounds (e.g., Technical Interview, Culture Fit), or is it a single set of fields per application for now?
2. Do we need an explicit "Offer Status" (Pending, Accepted, Declined) before moving to Archive, or does moving to Archive -> Hired/Declined suffice?
3. What specific fields should be captured at the "Applied" stage beyond the initial resume and contact info? (e.g., screening notes?)
