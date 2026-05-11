# Micro ATS - User Flows

This document details the screens and actions available to candidates and the system administrator.

## Candidate Experience (Public)

### 1. Job Board / List
- **View**: A simple list of all currently **Published** Job Postings.
- **Action**: Click on a Job Posting to view details.

### 2. Job Detail Page
- **View**: Displays the job title, department, description, and requirements.
- **Action**: Click "Apply Now" to open the application form.

### 3. Application Form
- **View**: A form collecting candidate details (Name, Email, Phone, Resume File).
- **Validation**: System prevents submission if the **Email** has already applied to this specific job.
- **Action**: Submit application (including file upload).
- **Result**: Candidate sees a "Thank You" confirmation page; a new **Application** is created in the system with the status **Applied**.

---

## Admin Experience (Internal)

### 1. Authentication
- **Screen**: Login page.
- **Action**: Admin enters email and password to access the internal dashboard.

### 2. Dashboard / Overview
- **View**: Summary of active job postings and recent applications.
- **Navigation**: Access to Requisitions, Job Postings, and Candidates.

### 3. Requisition Management
- **List View**: See all internal requisitions.
- **Action**: Create/Edit a Requisition.
- **Status Management**: Toggle between **Draft**, **Finalized**, and **Closed**.
- **Result**: Finalizing a Requisition **automatically** creates and publishes the **Job Posting**. Closing a Requisition **automatically closes** the associated Job Posting.

### 4. Job Posting Management
- **List View**: See all public-facing job advertisements.
- **Status Management**: Toggle between **Published** and **Closed**. 
- **Constraint**: Can be closed independently of the Requisition to stop new applications. However, closing the parent **Requisition** will automatically force this Job Posting to **Closed**.
- **Navigation**: Link back to the source Requisition for any content edits.

### 5. Candidate Tracking (Kanban/List)
- **View**: A board showing candidates grouped by their current stage:
  - **Applied**
  - **Interview**
  - **Offer**
  - **Archive**
- **Filter/Sort**: Ability to filter candidates by **Job Posting / Requisition**.
- **Search**: Search for candidates by **Name** or **Email**.
- **Action**: Click a candidate to view their full **Application** profile and download their resume file.
- **Action**: Move a candidate between stages. **Note**: Moving to **Archive** requires selecting a specific resolution status.

### 6. Candidate Review & Feedback
- **View**: Candidate's resume (file), contact info, and history of actions.
- **View**: History of all previously recorded **Feedback** (notes and scores).
- **Action**: Add new **Feedback** (textarea for notes + 1-5 score).

### 7. Offer Stage Communication
- **View**: Application details and negotiation history for candidates in the **Offer** stage.
- **Action**: Click **"Send Email"** button to discuss or send the offer.

### 8. Archive & Terminal Statuses
- **View**: List of candidates in the **Archive** stage, grouped or filtered by resolution: **Rejected**, **Hired**, **Declined**, or **Withdrawn**.
- **Requirement**: Each archived application **must** have one of these statuses assigned upon entry.
- **Action**: Click **"Send Email"** button to notify candidates of the final decision.

### 9. Offer Management
- **View**: Details of the formal proposal for a specific candidate.
- **Action**: Record offer details and track if it was accepted or declined.
- **Result**: Once decided, the application is moved to **Archive** with the appropriate status (Hired or Declined).
