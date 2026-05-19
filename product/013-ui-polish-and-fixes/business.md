# UI Polish and Bug Fixes

## Overview
Improve application consistency, fix reported UI bugs, and modernize interaction patterns (removing native browser dialogs).

## User Stories
- **As an Admin**, I want custom confirmation dialogs instead of native browser popups, so the experience feels modern and integrated.
- **As a User**, I want the current page to be highlighted in the navigation menu, so I always know my location in the app.
- **As a User**, I want all action buttons (Close, Publish, Finalize) to have a consistent visual style.
- **As an Admin**, I want to download candidate resumes without errors from any screen.
- **As an Admin**, I want the candidate profile page to be clean and free of redundant information.
- **As an Admin**, I want to select specific permissions for roles without affecting unrelated permissions.
- **As a User**, I want the "Change Password" button to be functional.

## Open Questions
- What should the "Change Password" button do? (For now, we will show a "Feature Coming Soon" notification or a simple mock dialog if we don't have a backend implementation ready, as per user's "button is doing nothing" report).
- Are there specific designs for the new confirmation dialog? (Will follow Honeycomb design system: centered modal with title, message, and primary/secondary buttons).
