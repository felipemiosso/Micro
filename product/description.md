# Micro ATS - Project Description

## Overview
A minimalist Applicant Tracking System (ATS) designed for small businesses to manage their hiring process. It centralizes job postings, application collection, and candidate evaluation in a single platform, replacing manual email-based tracking.

## Core Pillars

### 1. Job Management (Admin Only)
- Admin can create and manage the lifecycle of hiring needs starting from a Requisition.
- **Requisitions** handle the preparation phase (Draft -> Finalized).
- **Job Postings** represent the public manifestation of a Finalized Requisition and have two active states: **Published** and **Closed**.
- Job Postings and Requisitions can be closed independently (e.g., stop taking applications while still interviewing).
- Each published job generates a public landing page with a description and an application form.

### 2. Candidate Acquisition
- Public-facing forms for candidates to submit their details (Name, Email, Resume file).
- **Resume Handling**: Candidates must upload their resume as a file.
- **Duplicate Prevention**: The system prevents multiple applications from the same email address for the same job posting.
- Automatic ingestion of applications into the internal tracking system.
- Candidates receive an immediate on-screen confirmation upon successful submission.

### 3. Candidate Tracking (Manual)
- A central dashboard for the Admin to view and search for applicants.
- Manual progression through four fixed stages:
  1. **Applied**: Initial state for all new applications.
  2. **Interview**: Candidates selected for further evaluation.
  3. **Offer**: Active negotiation stage. Includes a manual "Send Email" action for offer communication.
  4. **Archive**: Terminal state for all finalized applications.
- **Archive Statuses**: Applications in Archive must specify a reason/type:
  - **Rejected**: Candidate did not pass evaluation.
  - **Hired**: Candidate accepted offer and was onboarded.
  - **Declined**: Candidate declined the offer.
  - **Withdrawn**: Candidate withdrew their application.
- **Candidate Communication**: For applications in **Offer** and **Archive** stages, the Admin has access to a manual "Send Email" action to communicate with candidates.
- Admin performs all screening and decision-making manually.

## Business Concepts

- **Requisition**: The internal starting point for hiring. It represents the intent to fill a position. It starts as a **Draft** while details are being finalized and becomes **Finalized** when ready to be advertised.
- **Job Posting**: The public advertisement of a **Finalized Requisition**. It is either **Published** (accepting applications) or **Closed** (no longer accepting applications).
- **Application**: The record of a candidate's interest in a specific job, containing their personal information, professional background, and resume file.
- **Feedback**: Qualitative and quantitative assessments of a candidate, including written notes and a numerical score (1 to 5).
- **Offer**: The formal proposal and negotiation record for a candidate who has successfully passed the evaluation process.

## Business Flow

- A **Requisition** is created as a **Draft**. Once all details (title, department, public description) are ready, it is **Finalized**.
- **Automatic Creation**: A Finalized Requisition automatically generates a **Published Job Posting**.
- The **Job Posting** stays **Published** to attract candidates and is eventually **Closed** (manually or automatically when the requisition is closed).
- **Closure Dependency**: Closing a **Requisition** automatically forces its associated **Job Posting** to close.
- Each published **Job Posting** collects unique **Applications**.
- As candidates move through the process, the admin records **Feedback** (notes + score) on their **Application**.
- Applications are eventually moved to **Offer** (negotiation) and finally to **Archive** with a specific resolution (Hired, Rejected, Declined, Withdrawn).
- Admins manually notify candidates in the **Offer** and **Archive** stages via the system.

## Rules & Constraints
- **User Roles**: Single "Admin" role using email and password authentication.
- **Evaluation Logic**: All screening and assessments are performed manually by the admin.
- **Integrations**: None for MVP beyond manual email trigger. Purely internal data management.
