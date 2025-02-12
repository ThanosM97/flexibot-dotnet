using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

using Shared.Events;
using Shared.Services;

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
        ILogger<RabbitMQConsumer> logger) : RabbitMQConsumerBase<DocumentUploadedEvent>(connection, "document_uploaded"), IAsyncDisposable
    {
        private readonly IConnection _connection = connection;
        private readonly MinioService _minioService = minioService;
        private readonly DocumentParser _parser = parser;
        private readonly ILogger<RabbitMQConsumer> _logger = logger;

        public override async Task StartConsumingAsync()
        {
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var message = DeserializeEvent(ea.Body.ToArray());
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
                        exchange: "",
                        routingKey: "document_parsed",
                        body: JsonSerializer.SerializeToUtf8Bytes(parsedEvent)
                    );

                    await Channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation($"Processed {message.FileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing document");
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