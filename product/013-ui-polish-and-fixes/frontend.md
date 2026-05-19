# Frontend Design - UI Polish

## Components

### ConfirmDialogComponent
Create a reusable standalone component in `src/app/core/ui/confirm-dialog/`.
- Inputs: `title`, `message`, `confirmText`, `cancelText`.
- Uses `MatDialog` for the modal behavior.
- Styles: Honeycomb theme (yellow/amber accents).

### Header Highlighting
Update `styles.css` to include styles for `.nav-links a.active`.
```css
.nav-links a.active {
  @apply text-honeycomb-400 font-bold border-b-2 border-honeycomb-500 pb-1;
}
```

## Feature Updates

### Requisition & Job Posting Lists
- Inject `MatDialog`.
- Replace `if (confirm(...))` with `dialog.open(ConfirmDialogComponent, ...)`.

### Candidate Details
- Fix CSS for `.detail-header` to handle single-child layout or add a right-side actions container.
- Remove the "Contact Info" card from the sidebar in `application-detail.html`.
- Fix `downloadResume` method to correctly handle the anchor tag.

### Role Editor
- The fix in the backend (unique permission strings) will automatically resolve the "both selected" issue as `isSelected(permission)` and `togglePermission(permission)` rely on unique strings.

### User Profile
- Add `(click)` handler to the "Change Password" div to show a notification.

### Action Styles
- Standardize button classes in `styles.css` or ensure all features use `btn-primary` / `btn-secondary` consistently without arbitrary `!py-1` overrides where not needed.
