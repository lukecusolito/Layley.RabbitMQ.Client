using System.Collections.Generic;
using System.Threading.Tasks;
using Layley.RabbitMQ.Client.Models;

namespace Layley.RabbitMQ.Client.Publishers
{
    public interface IRabbitPublisher
    {
        Task Publish(object message, string routingKey, string correlationId);
        Task Publish(List<BatchMessage> batchMessages);
    }
}
