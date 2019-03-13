using RabbitMQ.Client;

namespace Layley.RabbitMQ.Client.Connections
{
    public interface IRabbitConnectionFactory
    {
        IConnection CreateConnection();
    }
}
