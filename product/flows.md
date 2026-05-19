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
- **Screen**: Login page with password visibility toggle.
- **Action**: Admin enters credentials to access the dashboard.

### 2. Candidate Board (Kanban)
- **View**: A visual board with lanes for **Applied**, **Interview**, and **Offer**.
- **Interaction**: Drag and drop cards between lanes to progress candidates.
- **Archiving**: Dragging a card triggers a **floating bottom archive zone**. Dropping here opens a resolution dialog.
- **Deep Linking**: Profile links on Kanban cards navigate to the candidate detail page, automatically scrolling to and highlighting the specific application card.

### 3. Requisition & Job Management
- **Lists**: Tables with standardized status badges and action buttons.
- **Confirmation**: Deleting or closing items triggers a custom **ConfirmDialogComponent** instead of native popups.
- **Highlighting**: The current section is always highlighted in the sticky navigation header.

### 4. Candidate Profiles
- **View**: Integrated header with contact info and primary actions (Resume download).
- **Feedback**: Side-by-side management and history view.
- **Navigation**: "Back to Candidates" link returns to the board with state preserved.

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
