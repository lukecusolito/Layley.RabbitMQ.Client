using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Layley.RabbitMQ.Client.Subscribers
{
    public abstract class RabbitHandler
    {
        internal IConnection _connection;
        internal IModel _listeningChannel;
        internal IModel _deadLetterChannel;
        internal Dictionary<string, Func<object, Task>> listeners;
        internal EventingBasicConsumer consumer;

        /// <summary>Fires before the Subscriber's method is called</summary>
        public event EventHandler<SubscriptionEventArgs> BeforeExecution;

        /// <summary>Fires after the Subscriber's method is called</summary>
        public event EventHandler<SubscriptionEventArgs> AfterExecution;

        public RabbitHandler(IRabbitConnectionFactory connectionFactory)
        {
            listeners = new Dictionary<string, Func<object, Task>>();
            _connection = connectionFactory.CreateConnection();
            InitializeListeningChannel();
            InitializeDeadLetterQueue();
            InitializeSubscribers();
            RegisterConsumer();
        }

        protected abstract void InitializeSubscribers();

        private void InitializeDeadLetterQueue()
        {
            _deadLetterChannel = _connection.CreateModel();
            _deadLetterChannel.ExchangeDeclare(MessageBusConfiguration.DeadLetterExchange, "direct", true);
            //Dictionary<string, object> args = new Dictionary<string, object>();
            //args.Add("x-dead-letter-exchange", MessageBusConfiguration.DeadLetterExchange);
            _deadLetterChannel.QueueDeclare(MessageBusConfiguration.DeadLetterQueue, true, false, false, null); // args);
        }

        /// <summary>
        /// This method provides the fluent syntax to setup multiple routing patterns and subscribe their
        /// handler methods. Use: Subscribe["routing.pattern"] = async receivedMessage =&gt; { //Do Something with receivedMessage. }
        /// </summary>
        public TopicRouter Subscribe
        {
            get
            {
                return new TopicRouter(this);
            }
        }

        /// <summary>
        /// Declares a topic Exchange and a durable Queue to recieve messages.
        /// </summary>
        private void InitializeListeningChannel()
        {
            _listeningChannel = _connection.CreateModel();
            _listeningChannel.ExchangeDeclare(MessageBusConfiguration.ExchangeName, "topic", true);


            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("x-dead-letter-exchange", MessageBusConfiguration.DeadLetterExchange);
            //_listeningChannel.BasicQos(0, 1000, false);
            _listeningChannel.QueueDeclare(MessageBusConfiguration.QueueName, true, false, false, args);
        }

        /// <summary>
        /// Register a consumer event handler to route the incoming message to a delegate method using the routing key.
        /// </summary>
        protected void RegisterConsumer()
        {
            consumer = new EventingBasicConsumer(_listeningChannel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    string str = Encoding.UTF8.GetString(ea.Body);
                    string routingKey = ea.RoutingKey;
                    Func<object, Task> listener = listeners[routingKey];
                    SubscriptionEventArgs eventArgs = new SubscriptionEventArgs()
                    {
                        Message = (object)str,
                        BasicDeliverEventArgs = ea
                    };
                    if (listener == null)
                        return;
                    if (!await MethodWrapper<object>(eventArgs, listener))
                        return;
                    _listeningChannel.BasicAck(ea.DeliveryTag, false);
                }
                catch
                {
                    _listeningChannel.BasicNack(ea.DeliveryTag, false, false);
                }
            };
            _listeningChannel.BasicConsume(MessageBusConfiguration.QueueName, false, MessageBusConfiguration.ConsumerTag, consumer);
        }

        private async Task<bool> MethodWrapper<T>(SubscriptionEventArgs eventArgs, Func<T, Task> serviceMethod)
        {
            try
            {
                OnSubscriberExecuting(eventArgs);

                //var result = await serviceMethod?.Invoke((T)eventArgs.Message); // If result type is object
                await serviceMethod?.Invoke((T)eventArgs.Message); // Converting to void
                //if(MessageBusConfiguration.AutomaticRequeueIsEnabled)
                //Task.Run(() => RabbitRequeueMessageHandler.RequeueMessage(result, eventArgs.Message, eventArgs.CorrelationId));
            }
            catch (Exception ex)
            {
                eventArgs.ServiceCallException = ex;
                throw ex;
            }
            finally
            {
                OnSubscriberExecuted(eventArgs);
            }
            return true;
        }

        private void OnSubscriberExecuting(SubscriptionEventArgs eventArgs)
        {
            EventHandler<SubscriptionEventArgs> beforeExecution = BeforeExecution;
            if (beforeExecution == null)
                return;
            beforeExecution(null, eventArgs);
        }

        private void OnSubscriberExecuted(SubscriptionEventArgs eventArgs)
        {
            EventHandler<SubscriptionEventArgs> afterExecution = AfterExecution;
            if (afterExecution == null)
                return;
            afterExecution(null, eventArgs);
        }
        
        /// <summary>
        /// Binds a new routing key into the default queue with a defined function to execute on receive.
        /// </summary>
        /// <param name="routingKey">A <see cref="T:System.String" /> with the routing key to bind the queue to the exchange.</param>
        /// <param name="value">The delegate method to handle the incoming message.</param>
        private void Listen(string routingKey, Func<object, Task> value)
        {
            listeners.Add(routingKey, value);
            _listeningChannel.QueueBind(MessageBusConfiguration.QueueName, MessageBusConfiguration.ExchangeName, routingKey);
            _deadLetterChannel.QueueBind(MessageBusConfiguration.DeadLetterQueue, MessageBusConfiguration.DeadLetterExchange, routingKey);
        }

        public class TopicRouter
        {
            private readonly RabbitHandler _parentHandler;

            public TopicRouter(RabbitHandler parentHandler)
            {
                _parentHandler = parentHandler;
            }

            public Func<object, Task> this[string routingKey]
            {
                set
                {
                    Subscribe(routingKey, value);
                }
            }

            protected void Subscribe(string routingKey, Func<object, Task> value)
            {
                _parentHandler.Listen(routingKey, value);
            }
        }

        public class SubscriptionEventArgs : EventArgs
        {
            public object Message { get; set; }

            public Guid CorrelationId
            {
                get
                {
                    Guid guid = new Guid();
                    if (BasicDeliverEventArgs.BasicProperties.Headers.Keys.Contains("CorrelationId"))
                        guid = Guid.Parse(Encoding.UTF8.GetString(BasicDeliverEventArgs.BasicProperties.Headers["CorrelationId"] as byte[]));
                    return guid;
                }
            }

            public bool IsAck
            {
                get
                {
                    if (ServiceCallException == null)
                        return true;
                    return false;
                }
            }

            public BasicDeliverEventArgs BasicDeliverEventArgs { get; set; }

            public Exception ServiceCallException { get; set; }
        }
    }
}
