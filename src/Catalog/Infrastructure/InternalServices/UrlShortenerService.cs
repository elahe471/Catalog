using System.Net.Http.Json;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Catalog.Infrastructure.InternalServices;

public sealed class UrlShortenerService(HttpClient httpClient)
{
    public async Task<string> GetShortUrl(
        string longUrl,
        DateTime expirationDate,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "shortener")
        {
            Content = JsonContent.Create(new
            {
                longUrl,
                expirationDate
            })
        };

        request.Headers.Add( "Idempotency-Key",idempotencyKey);

        try
        {
            using var response = await httpClient.SendAsync(request,cancellationToken);

            // 200 or 201 is Ok 
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(
                    "UrlShortener",
                    StatusCodes.Status502BadGateway,
                    MessageCode.ExternalServiceFailure);
            }

            var shortUrl = await response.Content.ReadFromJsonAsync<string>( cancellationToken);

            return shortUrl;
        }
        catch (BrokenCircuitException ex)
        {
            throw new ExternalServiceException(
                "UrlShortener",
                StatusCodes.Status503ServiceUnavailable,
                MessageCode.ExternalServiceUnavailable,
                ex);
        }
        catch (TimeoutRejectedException ex)
        {
            throw new ExternalServiceException(
                "UrlShortener",
                StatusCodes.Status504GatewayTimeout,
                MessageCode.ExternalServiceTimeout,
                ex);
        }
        catch (OperationCanceledException ex)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExternalServiceException(
                "UrlShortener",
                StatusCodes.Status504GatewayTimeout,
                MessageCode.ExternalServiceTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException(
                "UrlShortener",
                StatusCodes.Status503ServiceUnavailable,
                MessageCode.ExternalServiceUnavailable,
                ex);
        }
    }
}