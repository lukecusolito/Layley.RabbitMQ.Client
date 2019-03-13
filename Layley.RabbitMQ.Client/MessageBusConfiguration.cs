namespace Layley.RabbitMQ.Client
{
    public static class MessageBusConfiguration
    {
        public static string HostName { get; set; }

        public static int Port { get; set; }

        public static string UserName { get; set; }

        public static string Password { get; set; }

        public static string VirtualHost { get; set; }

        public static string QueueName { get; set; }

        public static string ExchangeName { get; set; }

        public static string ConsumerTag { get; set; }

        internal static string DeadLetterExchange
        {
            get
            {
                return ExchangeName.Replace(".Exchange", $".{nameof(DeadLetterExchange)}");
            }
        }

        internal static string DeadLetterQueue
        {
            get
            {
                return QueueName.Replace(".Queue", $".{nameof(DeadLetterQueue)}");
            }
        }
    }
}
