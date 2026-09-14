namespace Catalog.Endpoints.Contracts;

public sealed record RemindItemRequest(int catalogItemId,int userId);
