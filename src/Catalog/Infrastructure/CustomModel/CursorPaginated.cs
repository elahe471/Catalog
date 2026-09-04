namespace Catalog.Infrastructure.CustomModel
{
    public record CursorPaginated<T>(
     IEnumerable<T> Items,
     int PageSize,
     bool HasNextPage,
     string? NextCursor);
}
