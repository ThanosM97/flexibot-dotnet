using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

using Shared.Events;


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
        protected readonly string Exchange;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQConsumerBase"/> class.
        /// </summary>
        /// <param name="connection">The RabbitMQ connection.</param>
        /// <param name="queueName">The name of the queue to consume from.</param>
        /// <param name="exchange">The name of the exchange to bind the queue to. Defaults to "documents".</param>
        protected RabbitMQConsumerBase(IConnection connection, string queueName, string exchange = "documents")
        {
            // Initialize the RabbitMQ connection and channel
            Connection = connection;
            Channel = connection.CreateChannelAsync().Result;

            // Set the queue and exchange names
            QueueName = queueName;
            Exchange = exchange;

            // Declare the exchange
            Channel.ExchangeDeclareAsync(exchange: Exchange, type: ExchangeType.Direct, durable: true);

            // Declare the queue
            Channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            // Bind the queue to the exchange
            Channel.QueueBindAsync(queue: queueName, exchange: Exchange, routingKey: queueName);
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

        /// <summary>
        /// Publishes a document status event to a message channel.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document.</param>
        /// <param name="status">The status of the document.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async Task PublishDocumentStatusEvent(string documentId, DocumentStatus status)
        {
            // Create a new DocumentStatusEvent with the provided document ID and status
            DocumentStatusEvent statusEvent = new (
                DocumentId: documentId,
                EventStatus: (int)status
            );

            // Publish the event to the specified exchange and routing key
            await Channel.BasicPublishAsync(
                exchange: Exchange,
                routingKey: "document_status",
                body: JsonSerializer.SerializeToUtf8Bytes(statusEvent)
            );
        }
    }
}
