using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Shared.Services
{
    /// <summary>
    /// Base class for publishing messages to a RabbitMQ exchange.
    /// Provides common functionality such as exchange declaration and message serialization.
    /// </summary>
    public class RabbitMQPublisher
    {
        protected readonly IChannel Channel;
        protected readonly string ExchangeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQPublisher"/> class.
        /// </summary>
        /// <param name="connection">The RabbitMQ connection.</param>
        /// <param name="exchangeName">The name of the exchange to publish to.</param>
        public RabbitMQPublisher(IConnection connection, string exchangeName = "")
        {
            ExchangeName = exchangeName;
            Channel = connection.CreateChannelAsync().Result;
            Channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Topic, durable: true);
        }

        /// <summary>
        /// Publishes a message to the configured exchange using the specified routing key.
        /// </summary>
        /// <typeparam name="T">The type of the message to publish.</typeparam>
        /// <param name="message">The message instance.</param>
        /// <param name="routingKey">The routing key for the message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task PublishAsync<T>(T message, string routingKey)
        {
            var body = SerializeEvent(message);

            Channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: null as BasicProperties,
                body: body
            );

            // Since BasicPublish is synchronous, we wrap the operation in a completed Task.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Serializes the specified message object to a byte array using UTF-8 encoding.
        /// </summary>
        /// <typeparam name="T">The type of the message object to serialize.</typeparam>
        /// <param name="message">The message object to be serialized.</param>
        /// <returns>A byte array representing the serialized message in JSON format.</returns>
        protected static byte[] SerializeEvent<T>(T message)
        {
            var json = JsonSerializer.Serialize(message);

            return Encoding.UTF8.GetBytes(json);
        }
    }
}
