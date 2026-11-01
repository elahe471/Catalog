namespace Catalog.Infrastructure.Exceptions;

public sealed class PriceGreaterThanZeroException : CatalogDomainException
{

    public PriceGreaterThanZeroException() : base(MessageTranslator.Translate(MessageCode.InvalidPrice))
    {
        
    }
}
