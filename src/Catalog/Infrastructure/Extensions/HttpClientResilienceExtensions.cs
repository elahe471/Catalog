using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Catalog.Infrastructure.Extensions
{
    public static class HttpClientResilienceExtensions
    {
        public static IHttpClientBuilder AddInternalServiceResilience(
     this IHttpClientBuilder builder,
     bool enablePostRetry = false)
        {
            builder.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(10);

                options.AttemptTimeout.Timeout =
                    TimeSpan.FromSeconds(3);

                options.Retry.MaxRetryAttempts = 2;
                options.Retry.Delay = TimeSpan.FromMilliseconds(500);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;

                if (enablePostRetry)
                {
                    // POST retries are allowed for idempotent operations.
                    options.Retry.DisableFor(
                        HttpMethod.Put,
                        HttpMethod.Patch,
                        HttpMethod.Delete,
                        HttpMethod.Connect);
                }
                else
                {
                    // Disable retries for unsafe HTTP methods.
                    options.Retry.DisableForUnsafeHttpMethods();
                }

                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 10;
                options.CircuitBreaker.SamplingDuration =
                    TimeSpan.FromSeconds(30);
                options.CircuitBreaker.BreakDuration =
                    TimeSpan.FromSeconds(15);
            });

            return builder;
        }
    }
}
