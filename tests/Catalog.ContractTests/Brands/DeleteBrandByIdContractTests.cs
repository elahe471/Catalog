using Catalog.Endpoints;
using Catalog.Infrastructure;
using Catalog.Models;
using Catalog.Services;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Catalog.ContractTests.Brands;

public class DeleteBrandByIdContractTests
{
    private CatalogServices SetupServices(string databaseName)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
                        .UseInMemoryDatabase(databaseName)
                        .Options;

        var loggerMock = new Mock<ILogger<CatalogServices>>();
        var publisherMock = new Mock<IPublishEndpoint>();

        var context = new CatalogDbContext(options);
        var mockServices = new CatalogServices(context,  loggerMock.Object, publisherMock.Object);

        return mockServices;
    }

    [Fact]
    public async Task DeleteBrandById_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var mockServices = SetupServices(databaseName: "DeleteBrandById_InvalidId_ReturnsBadRequest");

        // Act
        var results = await CatalogBrandEndpoints.DeleteBrandById(mockServices, -1, CancellationToken.None);

        // Assert
        var problem = results.Result.Should().BeOfType<ProblemHttpResult>().Which.ProblemDetails;
        problem.Status.Should().Be(400);
        problem.Detail.Should().Be("Id is not valid.");
        problem.Extensions["code"].Should().Be("InvalidId");
    }

    [Fact]
    public async Task DeleteBrandById_BrandNotFound_ReturnsNotFound()
    {
        // Arrange
        var mockServices = SetupServices(databaseName: "DeleteBrandById_BrandNotFound_ReturnsNotFound");

        // Act
        var results = await CatalogBrandEndpoints.DeleteBrandById(mockServices, 999, CancellationToken.None);

        // Assert
        var problem = results.Result.Should().BeOfType<ProblemHttpResult>().Which.ProblemDetails;
        problem.Status.Should().Be(404);
        problem.Detail.Should().Be("Brand with id 999 not found.");
    }

    [Fact]
    public async Task DeleteBrandById_ValidId_ReturnsNoContent()
    {
        // Arrange
        var mockServices = SetupServices(databaseName: "DeleteBrandById_ValidId_ReturnsNoContent");

        // Adding a brand to the in-memory database
        var context = mockServices.Context;
        var brand = CatalogBrand.Create("Test Brand");

        context.CatalogBrands.Add(brand);
        await context.SaveChangesAsync();

        // Act
        var results = await CatalogBrandEndpoints.DeleteBrandById(mockServices, brand.Id, CancellationToken.None);

        // Assert
        results.Result.Should().BeOfType<NoContent>();
        var brandFind = await context.CatalogBrands.FindAsync(brand.Id);
        brandFind.Should().BeNull();
    }
}
