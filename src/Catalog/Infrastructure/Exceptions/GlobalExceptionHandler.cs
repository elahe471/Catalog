using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Catalog.Infrastructure.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (context.Response.HasStarted || context.RequestAborted.IsCancellationRequested)
            return false;

        ProblemDetails problem;
        if (exception is ValidationException validation)
        {
            problem = new HttpValidationProblemDetails(validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).Distinct().ToArray()))
            {
                Status = StatusCodes.Status400BadRequest
            };
        }
        else
        {
            var (status, code) = exception switch
            {
                BadHttpRequestException badRequest => (badRequest.StatusCode, MessageCode.InvalidRequest),
                EmptyStockException => (StatusCodes.Status400BadRequest, MessageCode.EmptyStock),
                PriceGreaterThanZeroException => (StatusCodes.Status400BadRequest, MessageCode.InvalidPrice),
                QuantityGreaterThanZeroException => (StatusCodes.Status400BadRequest, MessageCode.InvalidQuantity),
                MaxStockThresholdGreaterThanZeroException => (StatusCodes.Status400BadRequest, MessageCode.InvalidMaxStockThreshold),
                CatalogDomainException => (StatusCodes.Status400BadRequest, MessageCode.DomainRuleViolation),
                ExternalServiceException external =>(external.StatusCode, external.Code),
                _ => (StatusCodes.Status500InternalServerError, MessageCode.UnexpectedError)
            };
            problem = new ProblemDetails
            {
                Status = status,
                Detail = exception is EmptyStockException ? exception.Message : MessageTranslator.Translate(code)
            };
            problem.Extensions["code"] = code.ToString();
            if (status >= 500)
            {
                if (exception is ExternalServiceException external)
                {
                    logger.LogError(
                        exception,
                        "External service {ServiceName} failed while processing {Path}",
                        external.ServiceName,
                        context.Request.Path);
                }
                else
                {
                    logger.LogError(
                        exception,
                        "Unhandled exception processing {Path}",
                        context.Request.Path);
                }
            }
        }

        problem.Title = ReasonPhrases.GetReasonPhrase(problem.Status!.Value);
        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        await TypedResults.Problem(problem).ExecuteAsync(context);
        return true;
    }
}
