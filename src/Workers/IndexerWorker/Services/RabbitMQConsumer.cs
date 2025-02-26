using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

using Shared.Events;
using Shared.Models;
using Shared.Services;
using System.Runtime.CompilerServices;


namespace IndexerWorker.Services
{
    /// <summary>
    /// RabbitMQ consumer service for processing document embedding events and indexing documents.
    /// </summary>
    /// <param name="connection">The RabbitMQ connection object.</param>
    /// <param name="indexer">The document indexer service.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public class RabbitMQConsumer(
        IConnection connection,
        DocumentIndexer indexer,
        ILogger<RabbitMQConsumer> logger) : RabbitMQConsumerBase<DocumentEmbeddedEvent>(connection, "document_embedded")
    {
        private readonly DocumentIndexer _indexer = indexer;
        private readonly ILogger<RabbitMQConsumer> _logger = logger;

        /// <summary>
        /// Starts consuming messages from the RabbitMQ queue.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task StartConsumingAsync()
        {
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.ReceivedAsync += async(model, ea) =>
            {
                // Get message from queue
                var message = DeserializeEvent(ea.Body.ToArray());

                try
                {
                    _logger.LogInformation($"Indexing document: {message.FileName}");

                    // Get chunks
                    List<DocumentChunk> chunks = message.Chunks;

                    // Index chunks
                    await _indexer.CreateCollectionIfNotExistsAsync();
                    await _indexer.IndexChunksAsync(message.FileName, chunks);

                    // Publish DocumentIndexedEvent
                    var indexedEvent = new DocumentIndexedEvent(
                        DocumentId: message.DocumentId,
                        ObjectStorageKey: message.ObjectStorageKey,
                        FileName: message.FileName,
                        IndexedAt: DateTime.UtcNow
                    );

                    // Publish document indexed event
                    await Channel.BasicPublishAsync(
                        exchange: "documents",
                        routingKey: "document_indexed",
                        body: JsonSerializer.SerializeToUtf8Bytes(indexedEvent)
                    );

                    // Acknowledge the message processing is complete
                    await Channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation($"Indexed document {message.FileName}");

                    // Publish indexed document status event
                    await PublishDocumentStatusEvent(message.DocumentId, DocumentStatus.Indexed);
                }
                catch (Exception ex)
                {
                    // Log the error and nack the message without requeueing
                    _logger.LogError(ex, "Error while indexing document");
                    await Channel.BasicNackAsync(ea.DeliveryTag, false, false);

                    // Publish failed document status event
                    await PublishDocumentStatusEvent(message.DocumentId, DocumentStatus.Failed);
                }
            };

            // Start consuming messages from the specified queue
            await Channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );
        }
    }
}