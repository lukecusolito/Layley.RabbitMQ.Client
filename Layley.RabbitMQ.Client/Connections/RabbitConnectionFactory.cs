using RabbitMQ.Client;
using System;

namespace Layley.RabbitMQ.Client.Connections
{
    public class RabbitConnectionFactory : IRabbitConnectionFactory
    {
        private IConnection _connection;

        public IConnection CreateConnection()
        {
            if (_connection == null)
            {
                ConnectionFactory connectionFactory = new ConnectionFactory();

                connectionFactory.HostName = MessageBusConfiguration.HostName;
                connectionFactory.Port = MessageBusConfiguration.Port;
                connectionFactory.UserName = MessageBusConfiguration.UserName;
                connectionFactory.Password = MessageBusConfiguration.Password;
                connectionFactory.VirtualHost = MessageBusConfiguration.VirtualHost;
                connectionFactory.AutomaticRecoveryEnabled = true;
                connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(15.0);
                connectionFactory.TopologyRecoveryEnabled = true;
                connectionFactory.RequestedHeartbeat = (ushort)30;

                _connection = connectionFactory.CreateConnection();
            }

            return _connection;
        }
    }
}
