using System.Text.Json;
using Catalog.Infrastructure.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catalog.UnitTests.Errors;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task UnexpectedException_ReturnsSafeProblem()
    {
        using var json = await Handle(new InvalidOperationException("Secret database credentials"), 500);
        json.RootElement.GetProperty("detail").GetString().Should().Be("An unexpected error occurred.");
        json.RootElement.GetProperty("code").GetString().Should().Be("UnexpectedError");
        json.RootElement.ToString().Should().NotContain("Secret database credentials");
    }

    [Fact]
    public async Task ValidationException_ReturnsFieldErrors()
    {
        var exception = new ValidationException(new[]
        {
            new ValidationFailure("Brand", "Brand is required."),
            new ValidationFailure("Brand", "Brand is required.")
        });
        using var json = await Handle(exception, 400);
        var errors = json.RootElement.GetProperty("errors").GetProperty("Brand");
        errors.GetArrayLength().Should().Be(1);
        errors[0].GetString().Should().Be("Brand is required.");
    }

    [Fact]
    public async Task MalformedRequest_ReturnsSafeBadRequest()
    {
        using var json = await Handle(new BadHttpRequestException("Internal binding detail"), 400);
        json.RootElement.GetProperty("detail").GetString().Should().Be("The request is invalid.");
    }

    [Fact]
    public async Task DomainException_ReturnsSpecificBusinessMessage()
    {
        using var json = await Handle(new PriceGreaterThanZeroException(), 400);
        json.RootElement.GetProperty("code").GetString().Should().Be("InvalidPrice");
        json.RootElement.GetProperty("detail").GetString().Should().Be("Item price desired should be greater than zero.");
    }

    private static async Task<JsonDocument> Handle(Exception exception, int expectedStatus)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        using var provider = services.BuildServiceProvider();
        using var body = new MemoryStream();
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Request.Path = "/api/v1/brands";
        context.Response.Body = body;
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(expectedStatus);
        context.Response.ContentType.Should().StartWith("application/problem+json");
        body.Position = 0;
        var json = await JsonDocument.ParseAsync(body, cancellationToken: TestContext.Current.CancellationToken);
        json.RootElement.GetProperty("instance").GetString().Should().Be("/api/v1/brands");
        json.RootElement.GetProperty("traceId").GetString().Should().Be(context.TraceIdentifier);
        return json;
    }
}
