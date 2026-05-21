# 017: Stage-Specific Application Data - Acceptance Criteria

## Interview Stage Data
- **AC-01** `[layer: e2e]`: When an application is in the `Interview` stage, the user can record an Interview Date, Interviewer Name, and general feedback (Notes + Score).
- **AC-02** `[layer: e2e]`: Interview data is displayed prominently in its own section on the Candidate Profile when the application reaches or passes the Interview stage.

## Offer Stage Data
- **AC-03** `[layer: e2e]`: When an application is in the `Offer` stage, the system displays an "Offer Details" form requiring Proposed Salary, Target Start Date, and Decision Deadline.
- **AC-04** `[layer: e2e]`: Users cannot archive an application as `Hired` from the Offer stage unless the mandatory Offer Details have been filled out.
- **AC-05** `[layer: integration]`: Saving Offer Details updates the application record successfully.

## Profile Display
- **AC-06** `[layer: e2e]`: The Candidate Profile displays a chronological timeline or structured view showing the data captured at each respective stage (Applied -> Interview -> Offer).
