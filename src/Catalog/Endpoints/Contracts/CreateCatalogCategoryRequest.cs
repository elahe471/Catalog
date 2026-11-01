namespace Catalog.Endpoints.Contracts;

public sealed record CreateCatalogCategoryRequest(string Category, int? ParentId);

public sealed class CreateCatalogCategoryRequestValidator : AbstractValidator<CreateCatalogCategoryRequest>
{
    public CreateCatalogCategoryRequestValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty()
            .NotNull()
            .MaximumLength(100);

        RuleFor(x => x.ParentId)
            .Must(x => !x.HasValue || (x.HasValue && x.Value > 0)).WithMessage(MessageTranslator.Translate(MessageCode.PositiveParentId));
    }
}