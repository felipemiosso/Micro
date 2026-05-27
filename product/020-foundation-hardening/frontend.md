# Spec 020: Foundation Hardening — Frontend Technical Design

## Tailwind Migration and Custom CSS Cleanups

To enforce strict alignment with the **Honeycomb Design System** and eliminate visual debt, we will migrate all custom layout/style declarations in component stylesheets to inline Tailwind CSS utility classes, delete the stylesheets, and clean up empty placeholder files.

### 1. Style Migrations

#### A. Job Application Form Layout
* **Source:** [apply.css](file:///C:/Users/felip/source/Micro/src/Micro.Web/src/app/features/applications/apply/apply.css)
* **Action:** Delete `apply.css` and remove `styleUrls: ['./apply.css']` from `apply.ts`.
* **Refactor:** Migrate the form alignment and layouts in [apply.html](file:///C:/Users/felip/source/Micro/src/Micro.Web/src/app/features/applications/apply/apply.html):
  * `.apply-container` -> `max-w-[600px] mx-auto my-8 p-8 bg-white rounded-lg shadow-md`
  * `.apply-header` -> `mb-8`
  * `.back-link` -> `inline-block mb-4 text-slate-500 hover:underline text-sm`
  * `h1` -> `text-3xl font-bold text-slate-800 m-0`
  * `.subtitle` -> `mt-1 text-slate-500 font-medium`
  * `.apply-form` -> `flex flex-col gap-6`
  * `.form-group` -> `flex flex-col gap-2`
  * `label` -> `font-semibold text-sm text-slate-700`
  * `.help-text` -> `text-xs text-slate-400 m-0`
  * `.alert-error` -> `p-4 rounded-md mb-6 text-sm bg-red-50 text-red-800 border border-red-200`
  * `.loading`, `.error-state` -> `text-center p-12 text-slate-500`

#### B. Application Detail View
* **Source:** [application-detail.css](file:///C:/Users/felip/source/Micro/src/Micro.Web/src/app/features/applications/detail/application-detail.css)
* **Action:** Delete `application-detail.css` and remove `styleUrls: ['./application-detail.css']` from `application-detail.ts`.
* **Refactor:** Migrate [application-detail.html](file:///C:/Users/felip/source/Micro/src/Micro.Web/src/app/features/applications/detail/application-detail.html) classes to Tailwind:
  * Replace `.status-0`, `.status-1`, `.status-2`, `.status-3` with Honeycomb Design System status classes:
    * `Applied` -> `badge-amber`
    * `Interview` -> `badge-blue`
    * `Offer` -> `badge-green`
    * `Archive` -> `badge-zinc`
  * Migrate `.detail-container`, `.detail-header`, `.info-row`, `.notes`, `.feedback-item` etc. to standard grid layout grids (`grid grid-cols-1 md:grid-cols-3 gap-6`), flexboxes, and borders.

#### C. Success Screen Layout
* **Source:** [success.css](file:///C:/Users/felip/source/Micro/src/Micro.Web/src/app/features/applications/success/success.css)
* **Action:** Delete `success.css` and remove `styleUrls: ['./success.css']` from `success.ts`.
* **Refactor:** Migrate [success.html](file:///C:/Users/felip/source/Micro/src/Micro.Web/src/app/features/applications/success/success.html):
  * `.success-container` -> `max-w-[500px] mx-auto mt-20 p-12 bg-white rounded-lg shadow-md text-center`
  * `.success-icon` -> `w-16 h-16 bg-green-500 text-white rounded-full text-3xl flex items-center justify-center mx-auto mb-8`
  * `h1` -> `text-2xl font-bold text-slate-800 mb-4`
  * `p` -> `text-slate-500 mb-8 leading-relaxed`

#### D. Job Posting Edit Page
* **Source:** [job-posting-edit.css](file:///C:/Users/felip/source/Micro/src/Micro.Web/src/app/features/job-postings/admin-edit/job-posting-edit.css)
* **Action:** Delete `job-posting-edit.css` and remove `styleUrls: ['./job-posting-edit.css']` from `job-posting-edit.ts`.
* **Refactor:** Migrate [job-posting-edit.html](file:///C:/Users/felip/source/Micro/src/Micro.Web/src/app/features/job-postings/admin-edit/job-posting-edit.html):
  * Replace the Bootstrap blue button `.btn-primary` (representing visual debt) and the grey `.btn-secondary` with standard Honeycomb `.btn-primary` and `.btn-secondary` classes.
  * Form group and inputs will use standard Tailwind utility classes for consistency.

#### E. Root Layout Height
* **Source:** [app.css](file:///C:/Users/felip/source/Micro/src/Micro.Web/src/app/app.css)
* **Action:** Delete `app.css` and remove `styleUrl: './app.css'` from `app.ts`.
* **Refactor:** In [app.html](file:///C:/Users/felip/source/Micro/src/Micro.Web/src/app/app.html), add `class="min-h-[calc(100vh-64px)]"` to the `<main>` tag.

---

### 2. Deleting Empty/Placeholder Stylesheets

We will remove 10 placeholder CSS files that contain no custom styling rules, and clean up their component declarations by removing the `styleUrl` or `styleUrls` property from their decorators:
1. `src/Micro.Web/src/app/features/applications/application-card/application-card.css`
2. `src/Micro.Web/src/app/features/applications/archived-applications/archived-applications.css`
3. `src/Micro.Web/src/app/features/applications/list/application-list.css`
4. `src/Micro.Web/src/app/features/auth/login/login.css`
5. `src/Micro.Web/src/app/features/auth/profile/profile.css`
6. `src/Micro.Web/src/app/features/job-postings/admin-list/job-posting-list.css`
7. `src/Micro.Web/src/app/features/job-postings/public-board/job-board.css`
8. `src/Micro.Web/src/app/features/job-postings/public-detail/job-detail.css`
9. `src/Micro.Web/src/app/features/requisitions/form/requisition-form.css`
10. `src/Micro.Web/src/app/features/requisitions/list/requisition-list.css`
