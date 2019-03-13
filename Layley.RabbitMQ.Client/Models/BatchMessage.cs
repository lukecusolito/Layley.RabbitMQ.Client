using System;

namespace Layley.RabbitMQ.Client.Models
{
    public class BatchMessage
    {
        #region Constructor
        public BatchMessage(string message, string routingKey, string correlationId)
        {
            Message = message;
            RoutingKey = routingKey;
            CorrelationId = correlationId;
        }

        public BatchMessage(string message, Enum routingKey, string correlationId)
        {
            Message = message;
            RoutingKey = routingKey.ToString();
            CorrelationId = correlationId;
        }
        #endregion

        public string Message { get; set; }
        public string RoutingKey { get; set; }
        public string CorrelationId { get; set; }
    }
}
