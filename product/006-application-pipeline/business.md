# Application Pipeline — Business Specification

### Overview
The Application Pipeline provides a visual Kanban-style board for recruiters and administrators to manage candidates efficiently. It transforms the list-based tracking into a dynamic interface where candidates are represented as cards across horizontal lanes corresponding to their hiring stage. 
Additionally, an Archived Applications Board provides a secondary view to track candidates whose applications have reached a terminal state, grouped by their resolution reason.

### User Stories
- As a **company administrator**, I want to see a Kanban board with lanes for 'Applied', 'Interview', and 'Offer', so that I can visualize the distribution of candidates across stages.
- As a **company administrator**, I want to see the total count of candidates at the top of each lane, so that I can quickly gauge the volume in each stage.
- As a **company administrator**, I want to drag a candidate's card from one lane to another in a sequential manner (Applied -> Interview -> Offer), so that the hiring process follows the established workflow.
- As a **company administrator**, I want to drag a candidate's card to an 'Archive' zone or click an archive action on the card, so that I can remove them from the active pipeline when a final decision is reached.
- As a **company administrator**, I want to be prompted for a resolution reason (Hired, Rejected, Declined, or Withdrawn) when archiving a candidate from the board, so that the archival data remains consistent.
- As a **company administrator**, I want to filter the board by job posting, so that I can focus on the pipeline for a specific role.
- As a **company administrator**, I want to access a secondary Kanban board for archived applications, with lanes for 'Hired', 'Rejected', 'Declined', and 'Withdrawn', so that I can review historical hiring outcomes.

### Constraints
- **Sequential Flow**: Candidates must move forward through the stages in order (Applied to Interview, Interview to Offer).

### Open Questions
- None.
