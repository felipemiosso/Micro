# 022: Pagination — Acceptance Criteria

## Page Navigation

**AC-01** `[layer: e2e]`
Given there are 25 requisitions in the system,
when the user opens the Requisitions list page,
then only the 20 most recent requisitions are shown, and the pagination controls display page 1 of 2.

**AC-02** `[layer: e2e]`
Given the user is on page 1 of the Requisitions list showing 20 items,
when the user clicks the "Next" button in the pagination controls,
then the list displays the remaining 5 older requisitions, and the pagination controls display page 2 of 2.

**AC-03** `[layer: e2e]`
Given the user is on page 2 of the Requisitions list,
when the user clicks the "Previous" button in the pagination controls,
then the list displays the first 20 most recent requisitions again.

---

## Page Size Adjustments

**AC-04** `[layer: e2e]`
Given there are 25 requisitions in the system,
and the page size is set to 20,
when the user changes the page size selector to 50,
then the list updates to show all 25 requisitions on a single page, and the pagination controls show page 1 of 1 (with page navigation buttons disabled).

---

## Interaction with Filtering and Searching

**AC-05** `[layer: e2e]`
Given there are 45 total applications,
and the user is on page 2 of the applications list (showing applications 21 to 40),
when the user types a search query or selects a status filter that matches only 5 applications,
then the search results are displayed, and the pagination active page is automatically reset to page 1.

---

## API Request Fallbacks

**AC-06** `[layer: integration]`
Given a client calls the paginated requisitions API endpoint,
when the request is made without providing `page` or `pageSize` parameters,
then the API defaults to page 1 and page size 20, returning a successful paged response.

**AC-07** `[layer: integration]`
Given a client calls the paginated requisitions API endpoint,
when the request contains an invalid page number (e.g. `page = 0` or `page = -5`),
then the API rejects the request with a `400 Bad Request` status and a validation error message.

**AC-08** `[layer: integration]`
Given a client calls the paginated requisitions API endpoint,
when the request contains an invalid page size (e.g. `pageSize = 0` or `pageSize = 150`),
then the API rejects the request with a `400 Bad Request` status and a validation error message.
