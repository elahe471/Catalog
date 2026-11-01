namespace Catalog.Endpoints;

public static class CatalogCategoryEndpoints
{
    public static IEndpointRouteBuilder MapCatalogCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateCategory);
        app.MapPut("/", UpdateCategory);
        app.MapDelete("/{id:int:required}", DeleteCategoryById);
        app.MapGet("/{id:int:required}", GetCategoryById);
        app.MapGet("/", GetCategories);

        return app;
    }

    public static async Task<Results<Created, ValidationProblem, ProblemHttpResult>> CreateCategory(
        [AsParameters] CatalogServices services,
        CreateCatalogCategoryRequest categoryToCreate,
        IValidator<CreateCatalogCategoryRequest> validator,
        CancellationToken cancellationToken)
    {
        var validate = validator.Validate(categoryToCreate);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        if (categoryToCreate.ParentId.HasValue)
        {
            var hasParent = await services.Context.CatalogCategories.AnyAsync(x => x.Id == categoryToCreate.ParentId, cancellationToken);
            if (!hasParent)
            {
                return ApiErrors.Problem(StatusCodes.Status400BadRequest, MessageCode.InvalidParentId);
            }
        }

        var hasCategory = await services.Context.CatalogCategories.AnyAsync(x => x.Category == categoryToCreate.Category &&
                                                                                 x.ParentId == categoryToCreate.ParentId,
                                                                                 cancellationToken);
        if (hasCategory)
        {
            return ApiErrors.Problem(StatusCodes.Status400BadRequest, MessageCode.CategoryAlreadyExists, categoryToCreate.Category);
        }

        var category = CatalogCategory.Create(categoryToCreate.Category, categoryToCreate.ParentId);

        services.Context.CatalogCategories.Add(category);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/categories/{category.Id}");
    }

    public static async Task<Results<Created, ValidationProblem, ProblemHttpResult>> UpdateCategory(
    [AsParameters] CatalogServices services,
    UpdateCatalogCategoryRequest categoryToUpdate,
    IValidator<UpdateCatalogCategoryRequest> validator,
    CancellationToken cancellationToken)
    {
        var validate = validator.Validate(categoryToUpdate);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        var category = await services.Context.CatalogCategories.FirstOrDefaultAsync(i => i.Id == categoryToUpdate.Id, cancellationToken);
        if (category is null)
        {
            return ApiErrors.Problem(StatusCodes.Status404NotFound, MessageCode.CategoryNotFound, categoryToUpdate.Id);
        }

        category.Update(categoryToUpdate.Category);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/categories/{category.Id}");
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> DeleteCategoryById
        ([AsParameters] CatalogServices services, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return ApiErrors.Problem(StatusCodes.Status400BadRequest, MessageCode.InvalidId);
        }

        var category = await services.Context.CatalogCategories
                                             .Include(ci => ci.Children)
                                             .FirstOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return ApiErrors.Problem(StatusCodes.Status404NotFound, MessageCode.CategoryNotFound, id);
        }

        if (category.Children.Any())
        { 
            return ApiErrors.Problem(StatusCodes.Status400BadRequest, MessageCode.CategoryHasChildren);
        }

        services.Context.CatalogCategories.Remove(category);
        await services.Context.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<CatalogCategoryResponse>, ProblemHttpResult>> GetCategoryById(
    [AsParameters] CatalogServices services,
    int id)
    {
        if (id <= 0)
        {
            return ApiErrors.Problem(StatusCodes.Status400BadRequest, MessageCode.InvalidId);
        }

        var category = await services.Context.CatalogCategories 
                                             .Include(ci => ci.Parent)             
                                             .FirstOrDefaultAsync(ci => ci.Id == id);
        if (category is null)
        {
            return ApiErrors.Problem(StatusCodes.Status404NotFound, MessageCode.CategoryNotFound, id);
        }

        return TypedResults.Ok(new CatalogCategoryResponse(id, category.Category, category.Path));
    }

    public static async Task<Results<Ok<IEnumerable<CatalogCategoryResponse>>, ProblemHttpResult>> GetCategories(
    [AsParameters] CatalogServices services,
    CancellationToken cancellationToken)
    {
        var categories = await services.Context.CatalogCategories
                                       .OrderBy(c => c.Id)
                                       .Select(x => new CatalogCategoryResponse(x.Id, x.Category, x.Path))
                                       .ToListAsync(cancellationToken);

        return TypedResults.Ok<IEnumerable<CatalogCategoryResponse>>(categories);
    }
}
