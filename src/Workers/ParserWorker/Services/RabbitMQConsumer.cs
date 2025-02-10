using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

using Shared.Events;

namespace ParserWorker.Services
{
    /// <summary>
    /// Consumes messages from a RabbitMQ queue, processes documents by downloading and parsing them,
    /// and publishes a new event to a different queue after successful parsing.
    /// </summary>
    /// <remarks>
    /// The class uses asynchronous operations for communication with RabbitMQ, MinIO, and document parsing.
    /// It establishes a connection to RabbitMQ upon construction, declares the necessary queue, and starts
    /// consuming messages when <see cref="StartConsumingAsync"/> is invoked.
    /// </remarks>
    public class RabbitMQConsumer : IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly MinioService _minioService;
        private readonly DocumentParser _parser;
        private readonly ILogger<RabbitMQConsumer> _logger;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQConsumer"/> class.
        /// </summary>
        /// <param name="config">The configuration settings containing RabbitMQ connection details.</param>
        /// <param name="minioService">The service used to interact with MinIO for file downloads.</param>
        /// <param name="parser">The document parser used to extract text content from the downloaded file.</param>
        /// <param name="logger">The logger for logging informational and error messages.</param>
        /// <remarks>
        /// The constructor creates a connection and a channel to RabbitMQ using asynchronous methods. It also declares
        /// a durable, non-exclusive, non-auto-deleting queue named "document_uploaded".
        /// </remarks>
        public RabbitMQConsumer(
            IConfiguration config,
            MinioService minioService,
            DocumentParser parser,
            ILogger<RabbitMQConsumer> logger)
        {
            _minioService = minioService;
            _parser = parser;
            _logger = logger;

            // Establish the connection asynchronously
            var factory = new ConnectionFactory
            {
                HostName = config.GetSection("RABBITMQ")["HOST"]
            };
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;

            _channel.QueueDeclareAsync(
                queue: "document_uploaded",
                durable: true,
                exclusive: false,
                autoDelete: false);
        }

        /// <summary>
        /// Starts consuming messages from the "document_uploaded" RabbitMQ queue.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method sets up an asynchronous event consumer using <see cref="AsyncEventingBasicConsumer"/>.
        /// For each received message, it performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     <description>Deserializes the message to a <see cref="DocumentUploadedEvent"/> object.</description>
        ///   </item>
        ///   <item>
        ///     <description>Downloads the document from MinIO using the <see cref="MinioService"/>.</description>
        ///   </item>
        ///   <item>
        ///     <description>Parses the document using the <see cref="DocumentParser"/> to extract text content.</description>
        ///   </item>
        ///   <item>
        ///     <description>Publishes a new <see cref="DocumentParsedEvent"/> to the "document_parsed" queue.</description>
        ///   </item>
        ///   <item>
        ///     <description>Acknowledges the message if processing is successful, or sends a negative acknowledgement in case of an error.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        public async Task StartConsumingAsync()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = JsonSerializer.Deserialize<DocumentUploadedEvent>(body);
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

                    var parsedEventBody = JsonSerializer.SerializeToUtf8Bytes(parsedEvent);
                    
                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: "document_parsed",
                        body: parsedEventBody
                    );

                    _logger.LogInformation($"Published parsed event for {message.FileName}");

                    // 4. Acknowledge message
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing document");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "document_uploaded",
                autoAck: false,
                consumer: consumer);
        }

        /// <summary>
        /// Performs asynchronous disposal of resources including the RabbitMQ channel and connection.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
        /// <remarks>
        /// It is important to properly close the RabbitMQ channel and connection to free up resources
        /// and ensure a graceful shutdown of the consumer.
        /// </remarks>
        public async ValueTask DisposeAsync()
        {
            // Asynchronously close the channel and connection
            await _channel.CloseAsync();
            await _connection.CloseAsync();
        }
    }
}
