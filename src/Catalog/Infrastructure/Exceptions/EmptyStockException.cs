namespace Catalog.Infrastructure.Exceptions;

public sealed class EmptyStockException : CatalogDomainException
{
    public EmptyStockException(string name) : base(MessageTranslator.Translate(MessageCode.EmptyStock, name))
    {

    }
}
