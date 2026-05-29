# 022: Pagination — Business Specification

## Overview

Currently, listing endpoints in Micro return all matching records at once. As the database grows, querying and rendering large lists of requisitions, job postings, and applications degrades API performance and client-side page load times.

To resolve this, we are introducing **offset-based pagination**:
1. Listing endpoints will accept query parameters `page` and `pageSize`.
2. The API will return list items alongside pagination metadata: `items`, `totalCount`, `page`, `pageSize`, and `totalPages`.
3. The frontend lists will implement pagination controls (previous/next page buttons, direct page links, page size selector) at the bottom of each list to navigate pages smoothly.
4. Target listing views:
   * **Requisitions**: Admin / hiring manager dashboard list.
   * **Applications**: Candidate applications tracking list.
   * **Public Job Board**: Public listing of published job postings.
   * **Admin Job Postings**: Admin view of job postings.

---

## User Stories

### Data Viewing (All Users)
* As a user, I want the lists of requisitions, job postings, and applications to load quickly, even when there are thousands of records in the system.
* As a user, I want to see how many total records match my search/filters and how many pages of results are available.
* As a user, I want to navigate between pages of results using pagination controls at the bottom of the table or card grid.
* As a user, I want to change the number of records shown per page (e.g. 10, 25, 50, or 100 items) to suit my screen size and browsing preference.

### Developer / API Consumer
* As an API consumer, I want consistent pagination query parameters and response schemas across all list endpoints.

---

## Open Questions

### Q-01: What should be the default and maximum page sizes?
* **Resolution**: The default page size across the system is 20. The UI will support choosing 10, 20, 50, or 100. The backend will enforce a maximum page size of 100 to prevent denial-of-service style huge queries.

### Q-02: How does pagination interact with searching and filtering?
* **Resolution**: Pagination applies to the filtered result set. When search text or filter criteria change, the list must reset back to page 1.
