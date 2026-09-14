

namespace Catalog.Infrastructure.Extensions;
public static class ApplicationExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContextPool<CatalogDbContext>(configure =>
        {
            configure.UseSqlServer(builder.Configuration.GetConnectionString(CatalogDbContext.DefaultConnectionStringName));
        });
        builder.Services.AddOptions<BrokerOptions>().BindConfiguration(BrokerOptions.SectionName);
        builder.Services.AddOptions<ShortenerOptions>().BindConfiguration(ShortenerOptions.SectionName);

        builder.Services.AddMassTransit(configure =>
        {
            var brokerConfig = builder.Configuration.GetRequiredSection(BrokerOptions.SectionName).Get<BrokerOptions>()!;

            configure.AddConsumers(Assembly.GetExecutingAssembly());
            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(brokerConfig.Host, hostConfigure =>
                {
                    hostConfigure.Username(brokerConfig.Username);
                    hostConfigure.Password(brokerConfig.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });




        builder.Services.AddHttpClient<UrlShortenerService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ShortenerOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });


        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
   
    }
}
