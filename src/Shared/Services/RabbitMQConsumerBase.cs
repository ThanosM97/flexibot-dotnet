// Shared/Services/RabbitMQConsumerBase.cs
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared.Services
{
    /// <summary>
    /// A generic base class for consuming messages from RabbitMQ.
    /// </summary>
    /// <typeparam name="TEvent">The type of event this consumer processes.</typeparam>
    public abstract class RabbitMQConsumerBase<TEvent> : IAsyncDisposable
    {
        protected readonly IConnection Connection;
        protected readonly IChannel Channel;
        protected readonly string QueueName;

        protected RabbitMQConsumerBase(IConnection connection, string queueName)
        {
            Connection = connection;
            Channel = connection.CreateChannelAsync().Result;
            QueueName = queueName;

            // Declare the exchange
            Channel.ExchangeDeclareAsync(exchange: "documents", type: ExchangeType.Direct, durable: true);

            // Declare the queue
            Channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            // Bind the queue to the exchange
            Channel.QueueBindAsync(queue: queueName, exchange: "documents", routingKey: queueName);
        }

        /// <summary>
        /// Start consuming messages asynchronously.
        /// </summary>
        public abstract Task StartConsumingAsync();

        /// <summary>
        /// Deserializes a message body into the specified event type.
        /// </summary>
        /// <param name="body">The raw message body bytes.</param>
        /// <returns>An instance of <typeparamref name="TEvent"/>.</returns>
        protected TEvent DeserializeEvent(byte[] body)
        {
            var json = Encoding.UTF8.GetString(body);
            return JsonSerializer.Deserialize<TEvent>(json)!;
        }

        public async ValueTask DisposeAsync()
        {
            await Channel.CloseAsync();
            await Connection.CloseAsync();
        }
    }
}
