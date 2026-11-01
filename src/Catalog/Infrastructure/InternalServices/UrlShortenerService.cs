
namespace Catalog.Infrastructure.InternalServices;

public class UrlShortenerService(HttpClient httpClient)
{
    public async Task<string> GetShortUrl(string longUrl,DateTime expirationDate,CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "shortener")
        {
            Content = JsonContent.Create(new
            {
                longUrl,
                expirationDate
            })
        };

        try
        {
            using var response = await httpClient.SendAsync( request, cancellationToken);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync(cancellationToken)
                : throw new ExternalServiceException("UrlShortener",StatusCodes.Status502BadGateway,MessageCode.ExternalServiceFailure);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExternalServiceException( "UrlShortener",StatusCodes.Status504GatewayTimeout,MessageCode.ExternalServiceTimeout,ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException("UrlShortener",StatusCodes.Status503ServiceUnavailable,MessageCode.ExternalServiceUnavailable,ex);
        }
    }
}