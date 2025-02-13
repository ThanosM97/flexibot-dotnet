using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

using Shared.Events;
using Shared.Services;

namespace ChunkerWorker.Services
{
    /// <summary>
    /// Consumes messages about parsed documents from a RabbitMQ queue, chunks the documents
    /// and publishes a new event to a different queue after successful chunking.
    /// </summary>
    /// <remarks>
    /// The class uses asynchronous operations for communication with RabbitMQ and document chunking.
    /// It declares the necessary queue on the established RabbitMQ connection, and starts
    /// consuming messages when <see cref="StartConsumingAsync"/> is invoked.
    /// </remarks>
    public class RabbitMQConsumer(
        IConnection connection,
        DocumentChunker chunker,
        ILogger<RabbitMQConsumer> logger) : RabbitMQConsumerBase<DocumentParsedEvent>(connection, "document_parsed"), IAsyncDisposable
    {
        private readonly IConnection _connection = connection;
        private readonly DocumentChunker _chunker = chunker;
        private readonly ILogger<RabbitMQConsumer> _logger = logger;

        public override async Task StartConsumingAsync()
        {
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.ReceivedAsync += async(model, ea) =>
            {
                try
                {
                    var message = DeserializeEvent(ea.Body.ToArray());
                    _logger.LogInformation($"Chunking document: {message.FileName}");

                    // Get text content
                    string textContent = message.ParsedTextContent;

                    // Chunk content
                    var chunkedContent = _chunker.Chunk(textContent, message.DocumentId);

                    // Publish DocumentChunkedEvent
                    var chunkedEvent = new DocumentChunkedEvent(
                        DocumentId: message.DocumentId,
                        ObjectStorageKey: message.ObjectStorageKey,
                        FileName: message.FileName,
                        Chunks: chunkedContent,
                        ChunkedAt: DateTime.UtcNow
                    );

                    await Channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: "document_chunked",
                        body: JsonSerializer.SerializeToUtf8Bytes(chunkedEvent)
                    );

                    await Channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation($"Chunked {message.FileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error chunking document");
                    await Channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await Channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );
        }

        public async ValueTask DisposeAsync()
        {
            await Channel.CloseAsync();
            await _connection.CloseAsync();
        }
    }
}