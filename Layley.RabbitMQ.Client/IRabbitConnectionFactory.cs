using RabbitMQ.Client;

namespace Layley.RabbitMQ.Client
{
    public interface IRabbitConnectionFactory
    {
        IConnection CreateConnection();
    }
}
