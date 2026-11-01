namespace Catalog.Infrastructure.Errors;

public enum MessageCode
{
    InvalidId, InvalidSlug, InvalidParentId, InvalidCategoryId, InvalidBrandId,
    BrandAlreadyExists, CategoryAlreadyExists, ItemAlreadyExists,
    BrandNotFound, CategoryNotFound, ItemNotFound, CategoryHasChildren,
    InvalidPageSize, InvalidCursor, ItemInStock, ReminderAlreadyExists,
    EmptyStock, InvalidPrice, InvalidQuantity, InvalidMaxStockThreshold,
    InvalidRequest, DomainRuleViolation, UnexpectedError, PositiveParentId, ServiceUnavailable
}

public static class MessageTranslator
{
    public static string Translate(MessageCode code, params object[] arguments)
    {
        var template = code switch
        {
            MessageCode.InvalidId => "Id is not valid.",
            MessageCode.InvalidSlug => "Slug is not valid.",
            MessageCode.InvalidParentId => "A parent Id is not valid.",
            MessageCode.InvalidCategoryId => "A category Id is not valid.",
            MessageCode.InvalidBrandId => "A brand Id is not valid.",
            MessageCode.BrandAlreadyExists => "A brand with the name '{0}' already exists.",
            MessageCode.CategoryAlreadyExists => "A category with the name '{0}' in this level already exists.",
            MessageCode.ItemAlreadyExists => "An item with the slug '{0}' already exists.",
            MessageCode.BrandNotFound => "Brand with id {0} not found.",
            MessageCode.CategoryNotFound => "Category with id {0} not found.",
            MessageCode.ItemNotFound => "Item with slug {0} not found.",
            MessageCode.CategoryHasChildren => "The category has child categories and cannot be deleted.",
            MessageCode.InvalidPageSize => "PageSize must be between 1 and 100.",
            MessageCode.InvalidCursor => "Invalid cursor.",
            MessageCode.ItemInStock => "Please make your order; the product is available in stock.",
            MessageCode.ReminderAlreadyExists => "A reminder has already been registered for this product.",
            MessageCode.EmptyStock => "Empty stock, product item {0} is sold out.",
            MessageCode.InvalidPrice => "Item price desired should be greater than zero.",
            MessageCode.InvalidQuantity => "Item units desired should be greater than zero.",
            MessageCode.InvalidMaxStockThreshold => "Item max stock threshold desired should be greater than zero.",
            MessageCode.InvalidRequest => "The request is invalid.",
            MessageCode.DomainRuleViolation => "A catalog business rule was violated.",
            MessageCode.ServiceUnavailable => "The service is temporarily unavailable.",
            MessageCode.UnexpectedError => "An unexpected error occurred.",
            MessageCode.PositiveParentId => "ParentId, if provided, must be greater than zero.",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
        return string.Format(System.Globalization.CultureInfo.CurrentCulture, template, arguments);
    }
}
