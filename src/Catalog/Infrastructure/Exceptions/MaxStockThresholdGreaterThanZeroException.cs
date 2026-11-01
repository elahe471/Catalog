namespace Catalog.Infrastructure.Exceptions;

public sealed class MaxStockThresholdGreaterThanZeroException : CatalogDomainException
{

    public MaxStockThresholdGreaterThanZeroException() : base(MessageTranslator.Translate(MessageCode.InvalidMaxStockThreshold))
    {
        
    }
}
