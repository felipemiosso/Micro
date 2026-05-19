# Micro ATS - Project Description

## Overview
A minimalist Applicant Tracking System (ATS) designed for small businesses to manage their hiring process. It centralizes job postings, application collection, and candidate evaluation in a single platform, replacing manual email-based tracking with a modern, visual workflow.

## Core Pillars

### 1. Job Management
- Admin can create and manage the lifecycle of hiring needs starting from a Requisition.
- **Requisitions** handle the preparation phase (Draft -> Finalized).
- **Job Postings** represent the public manifestation of a Finalized Requisition and have two active states: **Published** and **Closed**.
- Each published job generates a public landing page with a description and an application form.

### 2. Candidate Acquisition
- Public-facing forms for candidates to submit their details (Name, Email, Phone, Resume file).
- **Resume Handling**: Candidates upload PDF resumes, securely stored and accessible via the admin portal.
- **Duplicate Prevention**: The system prevents multiple applications from the same email address for the same job posting.

### 3. Candidate Tracking (Visual Kanban)
- **Applications Board**: A central Kanban-style dashboard for visual progression through stages:
  1. **Applied**: Initial state for all new applications.
  2. **Interview**: Candidates selected for further evaluation.
  3. **Offer**: Active negotiation stage.
- **Archive Process**: A Terminal state for all finalized applications. Archive triggers a floating drop-zone and requires a resolution (Hired, Rejected, Declined, Withdrawn).
- **Candidate Profiles**: Comprehensive view of all applications for a single candidate, including feedback history and resume downloads.

### 4. Security & Permissions
- **RBAC (Role-Based Access Control)**: Dynamic role creation with granular permissions (e.g., Application:View vs Application:Details).
- **Firebase Authentication**: Secure login with password visibility toggles and profile management.
- **Activity Logging**: Verbatim conversational logs for auditability.

## Business Concepts

- **Requisition**: The internal intent to fill a position.
- **Job Posting**: The public advertisement of a requisition.
- **Application**: The record of interest from a candidate.
- **Feedback**: Assessments (notes + score 1-5) recorded during evaluation.
- **Role**: A collection of permissions assigned to users.
- **Permission**: A specific action (View, Edit, Delete, Details) on a resource (Application, Requisition, JobPosting, Role).

## Business Flow

- **Draft -> Finalize**: Requisitions start as Drafts. Finalizing them automatically publishes a Job Posting.
- **Public Board**: Candidates browse and apply to published jobs.
- **Kanban Pipeline**: Admins drag and drop candidates through the pipeline.
- **Deep Linking**: Board cards link directly to specific applications on the profile page with automatic scrolling and highlighting.
- **Final Resolution**: Applications are moved to Archive when the process ends.
