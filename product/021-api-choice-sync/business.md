# 021: API Choice Sync — Business Specification

## Overview


For many fields and lookup tables (such as Custom Field "SingleChoice" options, "Departments", "Salary Bands", or "Cost Centers"), the list of selectable options changes frequently and is managed in external systems (like an HRIS or ERP). Manually updating these values in Micro is error-prone and inefficient.

To automate this, we are introducing **API Sync**:
1. External systems can sync a custom field's choices by sending a HTTP PUT request with the updated list of options.
2. External systems can also sync the master lookup data for **Departments**, **Salary Bands**, and **Cost Centers** via dedicated bulk sync endpoints.
3. All sync endpoints are authenticated using a secure API Key, avoiding the need for the external system to maintain a Firebase user account.
4. When a value is removed by the external system:
   * **Custom Field Choice**: If in use, it is archived into a `DisabledChoices` list. If not in use, it is deleted.
   * **Department**: If in use, it is marked inactive (`IsActive = false`). If not in use, it is deleted.
   * **Salary Band / Cost Center**: If in use by any requisition, it is left as-is to preserve database integrity. If not in use, it is deleted.
5. Archived/inactive values cannot be chosen for new records, but existing records retain their references/values.

---

## User Stories

### API Synchronization (External System)
* As an external system, I want to sync custom field choices, departments, salary bands, and cost centers via REST API endpoints using an API Key header (`X-API-Key`).
* As an external system, when I send a sync request, I want the system to accept the list of items, update existing ones, insert new ones, and handle retired items safely without throwing foreign key or validation errors.

### Administration (Admin)
* As an Admin, I want to configure the synchronization API Key in the application settings so that external systems can authenticate securely.

### Data Entry & Visibility (Hiring Managers / Recruiters / Candidates)
* As a user editing a form, when I open a dropdown containing synced options (Custom Fields, Departments, Salary Bands, or Cost Centers):
  * I should only be able to select from the currently active options.
  * If the record already has a retired/inactive option saved, I should see it rendered as selected, but I cannot select it again if I change the value.


---

## Open Questions

### Q-01: How should the API Key be configured and stored?
* **Resolution**: For simplicity and security in a single-instance setup, the API Key is configured in `appsettings.json` (e.g. `CustomFields:ApiKey`). If the header `X-API-Key` matches this configuration, the sync request is authorized.

### Q-02: What happens if a synced choice is re-added in a subsequent sync?
* **Resolution**: The choice is moved back from `DisabledChoices` to the active `Choices` list, making it selectable again for new records.
