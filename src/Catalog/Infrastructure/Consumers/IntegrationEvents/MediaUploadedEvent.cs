namespace Catalog.Infrastructure.Consumers.IntegrationEvents;

public record MediaUploadedEvent(string FileName, string Url, string CatalogId, DateTime OccurredOn);
