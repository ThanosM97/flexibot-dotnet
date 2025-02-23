using Microsoft.Extensions.Logging;
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
        private readonly ILogger<RabbitMQPublisher> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQPublisher"/> class.
        /// </summary>
        /// <param name="connection">The RabbitMQ connection.</param>
        /// <param name="exchangeName">The name of the exchange to publish to.</param>
        public RabbitMQPublisher(IConnection connection, ILogger<RabbitMQPublisher> logger, string exchangeName = "documents")
        {
            ExchangeName = exchangeName;
            _logger = logger;

            Channel = connection.CreateChannelAsync().Result;
            Channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);
        }

        /// <summary>
        /// Publishes a message to the configured exchange using the specified routing key.
        /// </summary>
        /// <typeparam name="T">The type of the message to publish.</typeparam>
        /// <param name="message">The message instance.</param>
        /// <param name="routingKey">The routing key for the message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task PublishAsync<T>(T message, string routingKey)
        {
            var body = SerializeEvent(message);

            try
            {
                var basicProperties = new BasicProperties();
                await Channel.BasicPublishAsync(
                    exchange: ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: basicProperties,
                    body: body
                );
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError($"Error occurred while publishing message: {ex}");
            }
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
