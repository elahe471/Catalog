using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Catalog.Infrastructure.Extensions
{
    public static class HttpClientResilienceExtensions
    {
        public static IHttpClientBuilder AddInternalServiceResilience( this IHttpClientBuilder builder)
        {
            builder.AddStandardResilienceHandler(options =>
            {
                // Maximum duration for the entire HTTP operation.
                options.TotalRequestTimeout.Timeout =TimeSpan.FromSeconds(10);

                // Maximum duration for each individual attempt.
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);

                // if the error is retryable, retry the operation only twice(with first one totaly 3 times) with a delay of 500 milliseconds between attempts.
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.Delay = TimeSpan.FromMilliseconds(500);
                // Increase retry delays exponentially.
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                // Use jitter to avoid retry storms when multiple clients are retrying at the same time.
                options.Retry.UseJitter = true;

                //prevent using retry on POST، PUT، PATCH، DELETE
                options.Retry.DisableForUnsafeHttpMethods();

                // Open the circuit when dependency failures exceed the threshold.
                // failure/success ratio of 50% or more active circuit breaker for 15 seconds, if at least 10 requests have been made in the last 30 seconds.
                options.CircuitBreaker.FailureRatio = 0.5;

                // Require at least 10 outcomes before evaluating failures.
                options.CircuitBreaker.MinimumThroughput = 10;

                // Evaluate failures within a rolling 30-second window.
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);

                // Keep the circuit open for 15 seconds before probing.
                options.CircuitBreaker.BreakDuration =TimeSpan.FromSeconds(15);
            });

            return builder;
        }
    }
}
