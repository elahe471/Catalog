
namespace Catalog.Infrastructure.Extensions
{
    public static class HealthCheckExtensions
    {
        public static void AddApplicationHealthChecks(this IHostApplicationBuilder builder)
        {

            builder.Services
                .AddHealthChecks()
                .AddDbContextCheck<CatalogDbContext>(
                    name: "EcommerceCatalog",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["ready"]);

        }

        public static IEndpointRouteBuilder MapApplicationHealthChecks(
            this IEndpointRouteBuilder endpoints)
        {
            // Liveness Check
            // Checks only whether the application is running and able to respond to requests.
            // It does NOT check external dependencies such as Database, RabbitMQ, or Elasticsearch.
            // Useful for determining whether the application process should be restarted.
            endpoints.MapHealthChecks(
                "/health/live",
                new HealthCheckOptions
                {
                    Predicate = _ => false,

                    ResponseWriter =
                        HealthCheckResponseWriter.WriteResponse
                });
            // Readiness Check
            // Checks whether this application instance is ready to receive traffic.
            // Only health checks tagged with "ready" are executed.
            //
            // Catalog dependencies:
            // - Database
            //
            // YARP or Kubernetes can use this endpoint to remove unhealthy instances
            // from load balancing until they become ready again.
            endpoints.MapHealthChecks(
                "/health/ready",
                new HealthCheckOptions
                {
                    Predicate = check =>
                        check.Tags.Contains("ready"),

                    ResponseWriter =
                        HealthCheckResponseWriter.WriteResponse
                });
            // Full Health Check
            // Executes all registered health checks for the application.
            // Useful for monitoring and troubleshooting.
            //
            // Includes:
            // - Database
            // - RabbitMQ
            // - Any other dependency health checks added in the future.
            endpoints.MapHealthChecks(
                "/health",
                new HealthCheckOptions
                {
                    ResponseWriter =
                        HealthCheckResponseWriter.WriteResponse
                });

            return endpoints;
        }
    }
}
