namespace Catalog.Infrastructure.CustomModel;

public record Paginated<T>(
 IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    bool HasNextPage);
