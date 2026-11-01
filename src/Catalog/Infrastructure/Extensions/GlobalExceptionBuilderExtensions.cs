namespace Catalog.Infrastructure.Extensions
{
    public static class GlobalExceptionBuilderExtensions
    {
        public static void ConfigureGlobalExceptions(this IHostApplicationBuilder builder)
        {

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
            {
                //"instance": "/api/products/123"
                context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
                //should be logged in the server side for troubleshooting
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            });
            // Throw exceptions for invalid request binding instead of automatically returning 400 Bad Request.
            builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
        }
    }
}
