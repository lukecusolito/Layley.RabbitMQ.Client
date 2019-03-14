using Layley.RabbitMQ.Client.Connections;
using Layley.RabbitMQ.Client.Subscribers;
using System;
using System.Threading.Tasks;

namespace Layley.RabbitMQ.Client.Core.TestHarness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IRabbitConnectionFactory connectionFactory = new RabbitConnectionFactory();

            MessageBusConfiguration.ExchangeName = "Test.Exchange";
            MessageBusConfiguration.HostName = "localhost";
            MessageBusConfiguration.Password = "guest";
            MessageBusConfiguration.Port = 5672;
            MessageBusConfiguration.QueueName = "Test.Queue";
            MessageBusConfiguration.UserName = "guest";
            MessageBusConfiguration.VirtualHost = "/";
            MessageBusConfiguration.ConsumerTag = "TestHarness";

            new TestHandler(connectionFactory);
        }
    }

    internal class TestHandler : RabbitHandler
    {
        public TestHandler(IRabbitConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
            BeforeExecution += QueueHandler_BeforeExecution;
            AfterExecution += QueueHandler_AfterExecution;
        }
        protected override void InitializeSubscribers()
        {
            Subscribe["activity.create"] = async message => await DoSomething(message.ToString());
        }

        private async Task DoSomething(string message)
        {

        }


        void QueueHandler_BeforeExecution(object sender, SubscriptionEventArgs e)
        {
            Console.WriteLine("Execute before handler");
        }

        void QueueHandler_AfterExecution(object sender, SubscriptionEventArgs e)
        {
            if (!e.IsAck)
            {
                Console.WriteLine("Execute after handler - notack");
            }
            else
            {
                Console.WriteLine("Execute after handler - ack");
            }
        }
    }
}
