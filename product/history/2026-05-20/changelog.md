## May 19 — UI Polish, Bug Fixes & Requisition Overhaul

  ### 🐛 UI Polish & Bug Fixes (Spec 013)
  • Replaced all native  confirm()  /  alert()  calls with the custom        
  ConfirmDialogComponent  (close requisition, publish requisition, close job)
  • Fixed active menu highlighting in the header
  • Fixed the download resume button (was broken on both candidate and
  application detail pages)
  • Cleaned up the Candidate page header — removed redundant contact info
  section, fixed phone number alignment
  • Standardized badge/action button styles across all pages
  • Added password visibility toggle (eye icon) to the login page
  • Fixed role permission grouping bug — clicking one permission was
  selecting multiple
  ### 👥 User Invitations (Spec 014)
  • Designed and implemented a full user management dashboard
  • Invite flow via Firebase's built-in password reset email (generates
  invite link)
  • Role assignment UI with inline table actions (no modal/dropdown)
  • Standardized table headers across all pages using Requisition as the
  reference
  ### 🏗️ Requisition Overhaul (Spec 015)
  • Extracted Department, SalaryBand, and CostCenter into their own
  normalized entities
  • Added per-opening  TargetStartDate  overrides (when openings > 1, each
  position can have its own date)
  • Built an Admin Settings page to manage all three lookup entities (full
  CRUD)
  • Added a  roleGuard  to protect admin routes
  • Fixed enum JSON serialization ( JsonStringEnumConverter ) —              
  workplaceType  was failing to deserialize
  • Improved seeding logic for all new lookup entities
  • Fixed all integration tests after schema reset
  ──────
  ## May 20 — Requisition Pages Adjustments (Spec 016)

  • Built out new Requisition detail/tracking view — showing all new fields,
  individual openings table, and navigation
  • Fixed enum display bug (frontend numeric enums vs backend string enums →
  status showed "Unknown")
  • Fixed empty date string bug — Angular was sending  ""  instead of  null 
  for cleared date fields, causing 400 errors on update
  • Added integration test: 
  Update_DraftRequisition_WithNullTargetStartDate_SavesNull 
  • Disabled "Update Requisition" button for non-Draft requisitions instead
  of showing a runtime error
  • Fixed  NG0100 ExpressionChangedAfterItHasBeenCheckedError  on the
  requisition edit page
  • Fixed Candidate application history showing "Unknown" for stage/status
  (backend was casting enums to  int , frontend expected strings)
  • Fixed application list status badges being hardcoded to  badge-slate  —
  now dynamic per status
  • Replaced legacy  *ngFor  with modern Angular  @for  control flow in the
  applications board