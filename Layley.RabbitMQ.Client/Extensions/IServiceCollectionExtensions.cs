using Microsoft.Extensions.DependencyInjection;
using Layley.RabbitMQ.Client.Connections;
using Layley.RabbitMQ.Client.Publishers;

namespace Layley.RabbitMQ.Client.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitClient(this IServiceCollection services) =>
            services
                .AddTransient<IRabbitConnectionFactory, RabbitConnectionFactory>()
                .AddTransient<IRabbitPublisher, RabbitPublisher>();
    }
}
