
using Catalog.Infrastructure.Helper;

namespace Catalog.Endpoints;

public static class CatalogItemEndpoints
{
    public static IEndpointRouteBuilder MapCatalogItemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateItem);
        app.MapPut("/", UpdateItem);
        app.MapPatch("/max_stock_threshold", UpdateMaxStockThreshold);
        app.MapDelete("/{slug:required}", DeleteItemBySlug);
        app.MapGet("/{slug:required}", GetItemBySlug);
        app.MapGet("/", GetItems);

        return app;
    }

    public static async Task<Results<Created, ValidationProblem, BadRequest<string>>> CreateItem(
        [AsParameters] CatalogServices services,
        CreateCatalogItemRequest itemToCreate,
        IValidator<CreateCatalogItemRequest> validator,
        IPublishEndpoint publishEndpoint,
        CancellationToken cancellationToken)
    {
        var validate = validator.Validate(itemToCreate);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        var hasCategory = await services.Context.CatalogCategories.AnyAsync(x => x.Id == itemToCreate.CatalogId, cancellationToken);
        if (!hasCategory)
        {
            return TypedResults.BadRequest($"A category Id is not valid.");
        }

        var hasBrand = await services.Context.CatalogBrands.AnyAsync(x => x.Id == itemToCreate.BrandId, cancellationToken);
        if (!hasBrand)
        {
            return TypedResults.BadRequest($"A brand Id is not valid.");
        }

        var hasItemSlug = await services.Context.CatalogItems.AnyAsync(x => x.Slug == itemToCreate.Name.ToKebabCase(), cancellationToken);
        if (hasItemSlug)
        {
            return TypedResults.BadRequest($"A Item with the slug '{itemToCreate.Name.ToKebabCase()}' already exists.");
        }

        var item = CatalogItem.Create(
            itemToCreate.Name,
            itemToCreate.Description,
            itemToCreate.MaxStockThreshold,
            itemToCreate.BrandId, itemToCreate.CatalogId);

        services.Context.CatalogItems.Add(item);
        await services.Context.SaveChangesAsync(cancellationToken);

        var detailUrl = $"/catalog/api/v1/items/{item.Slug}";
        var loadedItem = await services.Context.CatalogItems
                                                    .Include(ci => ci.CatalogBrand)
                                                    .Include(ci => ci.CatalogCategory)
                                                    .FirstAsync(x => x.Slug == item.Slug);

        await services.Publish.Publish(new CatalogItemAddedEvent(
                loadedItem.Name,
                loadedItem.Description,
                loadedItem.CatalogCategory.Category,
                loadedItem.CatalogBrand.Brand,
                loadedItem.Slug,
                detailUrl));

        return TypedResults.Created(detailUrl);
    }

    public static async Task<Results<Created, ValidationProblem, NotFound<string>, BadRequest<string>>> UpdateItem(
    [AsParameters] CatalogServices services,
    UpdateCatalogItemRequest itemToUpdate,
    IValidator<UpdateCatalogItemRequest> validator,
    CancellationToken cancellationToken)
    {
        var validate = validator.Validate(itemToUpdate);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        var Item = await services.Context.CatalogItems
                                                .FirstOrDefaultAsync(i => i.Slug == itemToUpdate.slug, cancellationToken);
        if (Item is null)
        {
            return TypedResults.NotFound($"Item with slug {itemToUpdate.slug} not found.");
        }

        var hasCategory = await services.Context.CatalogCategories.AnyAsync(x => x.Id == itemToUpdate.CatalogId, cancellationToken);
        if (!hasCategory)
        {
            return TypedResults.BadRequest($"A category Id is not valid.");
        }

        var hasBrand = await services.Context.CatalogBrands.AnyAsync(x => x.Id == itemToUpdate.BrandId, cancellationToken);
        if (!hasBrand)
        {
            return TypedResults.BadRequest($"A brand Id is not valid.");
        }

        Item.Update(itemToUpdate.Description,
                    itemToUpdate.BrandId,
                    itemToUpdate.CatalogId);

        await services.Context.SaveChangesAsync(cancellationToken);

        var loadedItem = await services.Context.CatalogItems
                                            .Include(ci => ci.CatalogBrand)
                                            .Include(ci => ci.CatalogCategory)
                                            .FirstAsync(x => x.Slug == Item.Slug);

        var detailUrl = $"/catalog/api/v1/items/{loadedItem.Slug}";

        await services.Publish.Publish(new CatalogItemChangedEvent(
                loadedItem.Name,
                loadedItem.Description,
                loadedItem.CatalogCategory.Category,
                loadedItem.CatalogBrand.Brand,
                loadedItem.Slug,
                detailUrl));

        return TypedResults.Created(detailUrl);
    }

    public static async Task<Results<Created, ValidationProblem, NotFound<string>, BadRequest<string>>> UpdateMaxStockThreshold(
    [AsParameters] CatalogServices services,
    UpdateCatalogItemMaxStockThresholdRequest itemToUpdate,
    IValidator<UpdateCatalogItemMaxStockThresholdRequest> validator,
    CancellationToken cancellationToken)
    {
        var validate = validator.Validate(itemToUpdate);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        var Item = await services.Context.CatalogItems.FirstOrDefaultAsync(i => i.Slug == itemToUpdate.Slug, cancellationToken);
        if (Item is null)
        {
            return TypedResults.NotFound($"Item with Slug {itemToUpdate.Slug} not found.");
        }

        Item.SetMaxStockThreshold(itemToUpdate.MaxStockThreshold);

        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/items/{Item.Slug}");
    }

    public static async Task<Results<NoContent, NotFound, BadRequest<string>>> DeleteItemBySlug(
    [AsParameters] CatalogServices services,
    string slug,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return TypedResults.BadRequest("Slug is not valid.");
        }

        var loadedItem = await services.Context.CatalogItems.FirstOrDefaultAsync(x => x.Slug == slug);
        if (loadedItem is null)
        {
            return TypedResults.NotFound();
        }

        services.Context.CatalogItems.Remove(loadedItem);
        await services.Context.SaveChangesAsync(cancellationToken);


        await services.Publish.Publish(new CatalogItemDeletedEvent(
                loadedItem.Slug));

        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<CatalogItemResponse>, NotFound, BadRequest<string>>> GetItemBySlug(
    [AsParameters] CatalogServices services,
    string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return TypedResults.BadRequest("Slug is not valid.");
        }

        var item = await services.Context.CatalogItems
                                         .Include(x => x.CatalogBrand)
                                         .Include(x => x.CatalogCategory)
                                         .Include(x => x.Medias)
                                         .FirstOrDefaultAsync(ci => ci.Slug == slug);
        if (item is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(
            new CatalogItemResponse(
                item.Name,
                item.Slug,
                item.Description,
                item.CatalogBrandId,
                item.CatalogBrand.Brand,
                item.CatalogCategoryId,
                item.CatalogCategory.Category,
                item.Price,
                item.AvailableStock,
                item.MaxStockThreshold, [.. item.Medias]));
    }
    public static async Task<
    Results<Ok<CursorPaginated<CatalogItemResponse>>, BadRequest<string>>> GetItems(
    [AsParameters] CatalogServices services,
    string? cursor = null,
    int pageSize = 20,
    CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageSize > 100)
            return TypedResults.BadRequest(
                "PageSize must be between 1 and 100.");

        CatalogCursor? decodedCursor = null;

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            decodedCursor = CursorHelper.Decode<CatalogCursor>(cursor);

            if (decodedCursor is null)
                return TypedResults.BadRequest("Invalid cursor.");
        }

        var query = services.Context.CatalogItems
            .AsNoTracking()
            .AsQueryable();

        if (decodedCursor is not null)
        {
            query = query.Where(x =>
                string.Compare(x.Name, decodedCursor.Name) > 0 ||
                (
                    x.Name == decodedCursor.Name &&
                    string.Compare(x.Slug, decodedCursor.Slug) > 0
                ));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Slug)
            .Take(pageSize + 1)
            .Select(x => new CatalogItemResponse(
                x.Name,
                x.Slug,
                x.Description,
                x.CatalogBrandId,
                x.CatalogBrand.Brand,
                x.CatalogCategoryId,
                x.CatalogCategory.Category,
                x.Price,
                x.AvailableStock,
                x.MaxStockThreshold,
                x.Medias.ToArray()))
            .ToListAsync(cancellationToken);

        var hasNextPage = items.Count > pageSize;

        if (hasNextPage)
            items.RemoveAt(items.Count - 1);

        string? nextCursor = null;

        if (hasNextPage && items.Count > 0)
        {
            var lastItem = items[^1];

            nextCursor = CursorHelper.Encode(
                new CatalogCursor(
                    lastItem.Name,
                    lastItem.Slug));
        }

        return TypedResults.Ok(
            new CursorPaginated<CatalogItemResponse>(
                items,
                pageSize,
                hasNextPage,
                nextCursor));
    }
}
