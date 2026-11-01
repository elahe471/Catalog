namespace Catalog.Infrastructure.Exceptions;

public sealed class QuantityGreaterThanZeroException : CatalogDomainException
{

    public QuantityGreaterThanZeroException() : base(MessageTranslator.Translate(MessageCode.InvalidQuantity))
    {
        
    }
}
