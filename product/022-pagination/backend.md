# 022: Pagination — Backend Design

## API Contracts & Helper Records

We will introduce a generic record `PagedResponse<T>` to structure the responses:

```csharp
namespace Micro.API.Infrastructure.Pagination;

public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
```

We will also define a pagination query model to represent requests:

```csharp
namespace Micro.API.Infrastructure.Pagination;

public record PaginationParams(
    int? Page,
    int? PageSize
)
{
    public int GetPage() => Page ?? 1;
    public int GetPageSize() => PageSize ?? 20;

    public bool IsValid(out string? errorMessage)
    {
        if (Page.HasValue && Page.Value < 1)
        {
            errorMessage = "Page number must be greater than or equal to 1.";
            return false;
        }
        if (PageSize.HasValue && (PageSize.Value < 1 || PageSize.Value > 100))
        {
            errorMessage = "Page size must be between 1 and 100.";
            return false;
        }
        errorMessage = null;
        return true;
    }
}
```

---

## Extension Methods for IQueryable

To keep endpoint handlers simple and clean without introducing repository classes, we will define an extension method in `Micro.API.Infrastructure.Pagination`:

```csharp
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Infrastructure.Pagination;

public static class PaginationExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize)
    {
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        if (totalPages == 0) totalPages = 1;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<T>(items, totalCount, page, pageSize, totalPages);
    }
    
    // For queries where transformations are done after materials (like including custom fields maps in DTOs)
    public static async Task<(List<T> Items, int TotalCount, int TotalPages)> GetPagedItemsAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize)
    {
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        if (totalPages == 0) totalPages = 1;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount, totalPages);
    }
}
```

---

## Endpoint Refactoring

All listing endpoints will perform the validation check:
```csharp
if (!pagination.IsValid(out var error))
{
    return Results.BadRequest(new { error });
}
```

The following endpoints will be refactored to support pagination parameters:

### 1. `GET /api/requisitions` (`RequisitionEndpoints.cs`)
* Modify endpoint signature to accept `[AsParameters] PaginationParams pagination`.
* Run validation check.
* Use the query after all filtering is applied.
* Map response:
```csharp
var (requisitions, totalCount, totalPages) = await query
    .OrderByDescending(r => r.CreatedAt)
    .GetPagedItemsAsync(pagination.GetPage(), pagination.GetPageSize());

// ... load custom field maps for this page only ...

var results = requisitions.Select(r => new { ... }).ToList();

return Results.Ok(new PagedResponse<object>(results, totalCount, pagination.GetPage(), pagination.GetPageSize(), totalPages));
```

### 2. `GET /api/applications` (`ApplicationEndpoints.cs`)
* Modify signature to accept `[AsParameters] PaginationParams pagination`, `ApplicationStatus? status`, and `ArchivalResolution? archivalResolution`.
* Filter the query using `status` and `archivalResolution` when provided.
* Run pagination on the list of admin applications.
* Map response to `PagedResponse<ApplicationDto>`.

### 3. `GET /api/jobs` (`JobPostingEndpoints.cs`)
* Modify signature to accept `[AsParameters] PaginationParams pagination`.
* Paginate the list of public job postings.

### 4. `GET /api/jobs/admin` (`JobPostingEndpoints.cs`)
* Modify signature to accept `[AsParameters] PaginationParams pagination`.
* Paginate the admin list of job postings.
