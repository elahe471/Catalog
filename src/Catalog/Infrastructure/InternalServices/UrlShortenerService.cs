namespace Catalog.Infrastructure.InternalServices;

public class UrlShortenerService(HttpClient httpClient)
{
    public async Task<string> GetShortUrl(
        string longUrl,
        DateTime expirationDate,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "shortener")
        {
            Content = JsonContent.Create(new
            {
                longUrl,
                expirationDate
            })
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}