namespace Catalog.Infrastructure.Extensions
{
    public static class BroakerBuilderExtenstions
    {
        public static void ConfigureBroker(this IHostApplicationBuilder builder)
        {
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
        }
    }
}
