

namespace Catalog.Infrastructure.Extensions;
public static class ApplicationBuilderExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContextPool<CatalogDbContext>(configure =>
        {
            configure.UseSqlServer(builder.Configuration.GetConnectionString(CatalogDbContext.DefaultConnectionStringName));
        });
   

        builder.Services.AddHttpClient<UrlShortenerService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ShortenerOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        }).AddInternalServiceResilience();

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
   
    }
}
