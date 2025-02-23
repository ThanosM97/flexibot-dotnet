using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

using Shared.Events;
using Shared.Services;

namespace EmbedderWorker.Services
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
        DocumentEmbedder embedder,
        ILogger<RabbitMQConsumer> logger) : RabbitMQConsumerBase<DocumentChunkedEvent>(connection, "document_chunked")
    {
        private readonly DocumentEmbedder _embedder = embedder;
        private readonly ILogger<RabbitMQConsumer> _logger = logger;

        public override async Task StartConsumingAsync()
        {
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.ReceivedAsync += async(model, ea) =>
            {
                try
                {
                    var message = DeserializeEvent(ea.Body.ToArray());
                    _logger.LogInformation($"Generating embeddings for document: {message.FileName}");

                    // Generate embeddings
                    await _embedder.EmbedChunksAsync(message.Chunks);

                    // Publish DocumentChunkedEvent
                    var embeddedEvent = new DocumentEmbeddedEvent(
                        DocumentId: message.DocumentId,
                        ObjectStorageKey: message.ObjectStorageKey,
                        FileName: message.FileName,
                        Chunks: message.Chunks,
                        EmbeddedAt: DateTime.UtcNow
                    );

                    // Publish document embedded event
                    await Channel.BasicPublishAsync(
                        exchange: "documents",
                        routingKey: "document_embedded",
                        body: JsonSerializer.SerializeToUtf8Bytes(embeddedEvent)
                    );

                    await Channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation($"Generated embeddings for {message.FileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while generating embeddings for document");
                    await Channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await Channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );
        }
    }
}