using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Layley.RabbitMQ.Client.Connections;
using Layley.RabbitMQ.Client.Models;

namespace Layley.RabbitMQ.Client.Publishers
{
    public class RabbitPublisher : IRabbitPublisher
    {
        private readonly IRabbitConnectionFactory _connectionFactory;

        public RabbitPublisher(IRabbitConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public virtual async Task Publish(object message, string routingKey, string correlationId)
        {
            await Task.Run(() =>
            {
                using (IModel model = _connectionFactory.CreateConnection().CreateModel())
                {
                    model.ExchangeDeclare(MessageBusConfiguration.ExchangeName, "topic", true);
                    IBasicProperties basicProperties = model.CreateBasicProperties();
                    basicProperties.DeliveryMode = 2;
                    basicProperties.Headers = new Dictionary<string, object>()
                    {
                        {
                          nameof (correlationId),
                          correlationId
                        }
                    };
                    var body = Encoding.UTF8.GetBytes(message.ToString());
                    model.BasicPublish(MessageBusConfiguration.ExchangeName, routingKey, basicProperties, body);
                }
            });
        }

        public virtual async Task Publish(List<BatchMessage> batchMessages)
        {
            await Task.Run(() =>
            {
                using (IModel model = _connectionFactory.CreateConnection().CreateModel())
                {
                    var basicPublishBatch = model.CreateBasicPublishBatch();

                    foreach(var message in batchMessages)
                    {
                        IBasicProperties basicProperties = model.CreateBasicProperties();
                        basicProperties.DeliveryMode = 2;
                        basicProperties.Headers = new Dictionary<string, object>
                        {
                            { nameof(message.CorrelationId), message.CorrelationId }
                        };

                        var body = Encoding.UTF8.GetBytes(message.Message);
                        basicPublishBatch.Add(MessageBusConfiguration.ExchangeName, message.RoutingKey, false, basicProperties, body);
                    }
                    basicPublishBatch.Publish();
                }
            });
        }
    }
}
