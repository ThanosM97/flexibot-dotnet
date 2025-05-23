using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

using Shared.Events;
using Shared.Services;
using Shared.Services.Storage;

namespace ParserWorker.Services
{
    /// <summary>
    /// Consumes messages from a RabbitMQ queue, processes documents by downloading and parsing them,
    /// and publishes a new event to a different queue after successful parsing.
    /// </summary>
    /// <remarks>
    /// The class uses asynchronous operations for communication with RabbitMQ, MinIO, and document parsing.
    /// It declares the necessary queue on the established RabbitMQ connection, and starts
    /// consuming messages when <see cref="StartConsumingAsync"/> is invoked.
    /// </remarks>
    public class RabbitMQConsumer(
        IConnection connection,
        MinioService minioService,
        DocumentParser parser,
        ILogger<RabbitMQConsumer> logger) : RabbitMQConsumerBase<DocumentUploadedEvent>(connection, "document_uploaded")
    {
        private readonly MinioService _minioService = minioService;
        private readonly DocumentParser _parser = parser;
        private readonly ILogger<RabbitMQConsumer> _logger = logger;

        public override async Task StartConsumingAsync()
        {
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                // Get message from queue
                var message = DeserializeEvent(ea.Body.ToArray());

                try
                {
                    _logger.LogInformation($"Processing document: {message.FileName}");

                    // Download file from MinIO
                    using var fileStream = await _minioService.DownloadFileAsync(message.ObjectStorageKey);

                    // Parse document
                    var textContent = _parser.ParseDocument(fileStream, message.FileName);

                    // Publish DocumentParsedEvent
                    var parsedEvent = new DocumentParsedEvent(
                        DocumentId: message.DocumentId,
                        ObjectStorageKey: message.ObjectStorageKey,
                        FileName: message.FileName,
                        ParsedAt: DateTime.UtcNow,
                        ParsedTextContent: textContent
                    );
                    await Channel.BasicPublishAsync(
                        exchange: "documents",
                        routingKey: "document_parsed",
                        body: JsonSerializer.SerializeToUtf8Bytes(parsedEvent)
                    );

                    await Channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation($"Processed {message.FileName}");

                    // Publish parsed document status event
                    await PublishDocumentStatusEvent(message.DocumentId, DocumentStatus.Parsed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing document");
                    await Channel.BasicNackAsync(ea.DeliveryTag, false, false);

                    // Publish failed document status event
                    await PublishDocumentStatusEvent(message.DocumentId, DocumentStatus.Failed);
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