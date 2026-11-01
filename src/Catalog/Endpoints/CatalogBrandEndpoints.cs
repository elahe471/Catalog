namespace Catalog.Endpoints;

public static class CatalogBrandEndpoints
{
    public static IEndpointRouteBuilder MapCatalogBrandEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateBrand);
        app.MapPut("/", UpdateBrand);
        app.MapDelete("/{id:int:required}", DeleteBrandById);
        app.MapGet("/{id:int:required}", GetBrandById);
        app.MapGet("/", GetBrands);

        return app;
    }

    public static async Task<Results<Created, ValidationProblem, ProblemHttpResult>> CreateBrand(
        [AsParameters] CatalogServices services,
        CreateCatalogBrandRequest brandToCreate,
        IValidator<CreateCatalogBrandRequest> validator,
        CancellationToken cancellationToken)
    {
        var validate = validator.Validate(brandToCreate);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        var hasBrand = await services.Context.CatalogBrands.AnyAsync(x => x.Brand == brandToCreate.Brand, cancellationToken);
        if (hasBrand)
        {
            return ApiErrors.Problem(StatusCodes.Status400BadRequest, MessageCode.BrandAlreadyExists, brandToCreate.Brand);
        }

        var brand = CatalogBrand.Create(brandToCreate.Brand);

        services.Context.CatalogBrands.Add(brand);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/brands/{brand.Id}");
    }

    public static async Task<Results<Created, ValidationProblem, ProblemHttpResult>> UpdateBrand(
    [AsParameters] CatalogServices services,
    UpdateCatalogBrandRequest brandToUpdate,
    IValidator<UpdateCatalogBrandRequest> validator,
    CancellationToken cancellationToken)
    {
        var validate = validator.Validate(brandToUpdate);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        var brand = await services.Context.CatalogBrands.FirstOrDefaultAsync(i => i.Id == brandToUpdate.Id, cancellationToken);
        if (brand is null)
        {
            return ApiErrors.Problem(StatusCodes.Status404NotFound, MessageCode.BrandNotFound, brandToUpdate.Id);
        }

        brand.Update(brandToUpdate.Brand);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/brands/{brand.Id}");
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> DeleteBrandById
        ([AsParameters] CatalogServices services, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return ApiErrors.Problem(StatusCodes.Status400BadRequest, MessageCode.InvalidId);
        }

        var brand = await services.Context.CatalogBrands.FirstOrDefaultAsync(x => x.Id == id);
        if (brand is null)
        {
            return ApiErrors.Problem(StatusCodes.Status404NotFound, MessageCode.BrandNotFound, id);
        }

        services.Context.CatalogBrands.Remove(brand);
        await services.Context.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<CatalogBrandResponse>, ProblemHttpResult>> GetBrandById(
    [AsParameters] CatalogServices services,
    int id)
    {
        if (id <= 0)
        {
            return ApiErrors.Problem(StatusCodes.Status400BadRequest, MessageCode.InvalidId);
        }

        var brand = await services.Context.CatalogBrands.FirstOrDefaultAsync(ci => ci.Id == id);
        if (brand is null)
        {
            return ApiErrors.Problem(StatusCodes.Status404NotFound, MessageCode.BrandNotFound, id);
        }

        return TypedResults.Ok(new CatalogBrandResponse(id, brand.Brand));
    }

    public static async Task<Results<Ok<IEnumerable<CatalogBrandResponse>>, ProblemHttpResult>> GetBrands(
    [AsParameters] CatalogServices services,
    CancellationToken cancellationToken)
    {
        var brands = await services.Context.CatalogBrands
                                           .OrderBy(c => c.Id)
                                           .Select(x => new CatalogBrandResponse(x.Id, x.Brand))
                                           .ToListAsync(cancellationToken);

        return TypedResults.Ok<IEnumerable<CatalogBrandResponse>>(brands);
    }
}
