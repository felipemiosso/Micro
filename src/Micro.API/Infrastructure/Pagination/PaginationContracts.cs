using System.Collections.Generic;

namespace Micro.API.Infrastructure.Pagination;

public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

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
