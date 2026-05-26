# 019: Selectable Custom Fields — Frontend Design

## Angular Services Update

### `custom-field.service.ts` [Modified]
Add new methods to interact with linking/unlinking endpoints:
```typescript
import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustomFieldDefinition } from './custom-field.types';

@Injectable({ providedIn: 'root' })
export class CustomFieldService {
  constructor(private http: HttpClient) {}

  // Get selectable fields templates from pool
  getSelectableFields(targetEntity?: string): Observable<CustomFieldDefinition[]> {
    const url = targetEntity 
      ? `/api/custom-fields/selectable?targetEntity=${targetEntity}`
      : '/api/custom-fields/selectable';
    return this.http.get<CustomFieldDefinition[]>(url);
  }

  // Requisition associations
  linkRequisitionField(requisitionId: string, definitionId: string): Observable<void> {
    return this.http.post<void>(`/api/requisitions/${requisitionId}/custom-fields`, {
      customFieldDefinitionId: definitionId
    });
  }

  unlinkRequisitionField(requisitionId: string, definitionId: string): Observable<void> {
    return this.http.delete<void>(`/api/requisitions/${requisitionId}/custom-fields/${definitionId}`);
  }

  // Job Posting associations
  linkJobPostingField(jobPostingId: string, definitionId: string): Observable<void> {
    return this.http.post<void>(`/api/job-postings/${jobPostingId}/custom-fields`, {
      customFieldDefinitionId: definitionId
    });
  }

  unlinkJobPostingField(jobPostingId: string, definitionId: string): Observable<void> {
    return this.http.delete<void>(`/api/job-postings/${jobPostingId}/custom-fields/${definitionId}`);
  }
}
```

---

## UI Components

### Custom Field Assignment Widget (`field-assignment.ts` / `field-assignment.html`) [NEW]
A reusable component to manage optional custom fields assigned to a specific requisition or job posting.

#### Template layout (`field-assignment.html`)
*   **Header**: Title "Additional Custom Fields" + description.
*   **Active List**: A list of currently active fields.
    *   Global fields: Render with a grey/neutral badge `Global` (no unlink button).
    *   Selected fields: Render with a blue badge `Selected` + an "Unlink" icon button.
*   **Add Button**: A button "Add Custom Field" that toggles a drop-down or dialog of available selectable fields from the pool.
*   **Add Panel/Dropdown**:
    *   Lists definitions matching the entity context (for Requisitions, only Requisition-target fields. For Job Postings, both JobPosting-target and Application-target fields).
    *   If no selectable fields are available in the pool, shows "No fields available in pool".

```html
<div class="card p-4 border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 rounded-lg">
  <div class="flex items-center justify-between mb-4">
    <h3 class="text-lg font-semibold text-slate-900 dark:text-white">Custom Fields</h3>
    <button (click)="openAddModal()" class="btn btn-primary btn-sm flex items-center gap-1">
      <span class="material-icons text-sm">add</span> Add Field
    </button>
  </div>

  <!-- Fields List -->
  <div class="space-y-2">
    @for (field of activeFields(); track field.id) {
      <div class="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-950 border border-slate-100 dark:border-slate-900 rounded">
        <div>
          <span class="font-medium text-slate-800 dark:text-slate-200">{{ field.label }}</span>
          <span class="text-xs text-slate-500 ml-2">({{ field.fieldType }})</span>
        </div>
        <div class="flex items-center gap-2">
          @if (field.isGlobal) {
            <span class="badge-status badge-slate">Global</span>
          } @else {
            <span class="badge-status badge-blue">Selected</span>
            <button (click)="confirmUnlink(field)" class="text-red-500 hover:text-red-700 p-1 rounded hover:bg-slate-100 dark:hover:bg-slate-900">
              <span class="material-icons text-sm">link_off</span>
            </button>
          }
        </div>
      </div>
    } @empty {
      <p class="text-sm text-slate-500 italic">No custom fields active.</p>
    }
  </div>
</div>
```

---

## Form Integration & Dynamic State

### Reacting to Field Additions/Deletions
When a selectable field is linked or unlinked, the editing component must refresh its local list of active custom field definitions:

1.  **Retrieve Updated Definitions**: Fetch the entity's active fields list from the backend (which now automatically merges global + linked).
2.  **Synchronize Reactive Form Group**:
    *   When a new field is linked: Instantly create and insert the new form control matching the field type and validation rules into the `customFields` Form Group.
    *   When unlinked: Remove the form control from the Form Group to prevent it from submitting on save.
3.  **UI Feedback**: Use `NotificationService` to display success on linking/unlinking, and error banners if an unlink request fails (e.g., due to existing values).

### Candidate Dynamic Form Integration
No changes are required on the candidate public application page itself. Because the backend public job-posting endpoint automatically resolves the union of global and job-scoped candidate-facing fields, the dynamic form in the public portal will automatically render the new fields without code modifications.
