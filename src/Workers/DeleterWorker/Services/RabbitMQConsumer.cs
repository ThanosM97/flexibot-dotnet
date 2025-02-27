using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Shared.Events;
using Shared.Services;


namespace DeleterWorker.Services
{
    /// <summary>
    /// Consumer service for processing document deletion events from RabbitMQ.
    /// </summary>
    /// <param name="connection">The RabbitMQ connection object.</param>
    /// <param name="documentDeleter">Service for deleting documents and vector points.</param>
    /// <param name="logger">Logger for logging information and errors.</param>
    public class RabbitMQConsumer(
        IConnection connection,
        DocumentDeleter documentDeleter,
        ILogger<RabbitMQConsumer> _logger
    ) : RabbitMQConsumerBase<DocumentDeletedEvent>(connection, "document_deleted")
    {
        /// <summary>
        /// Starts the asynchronous consumption of document deletion events.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task StartConsumingAsync()
        {
            // Create a new asynchronous event consumer for the channel
            var consumer = new AsyncEventingBasicConsumer(Channel);

            // Attach an event handler to process received messages
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    // Deserialize the event message from the message body
                    var message = DeserializeEvent(ea.Body.ToArray());
                    _logger.LogInformation($"Processing delete event: {message.DocumentId}");

                    // Delete document from storage
                    await documentDeleter.DeleteDocumentAsync(message.DocumentId);
                    _logger.LogInformation($"Deleted document {message.DocumentId} from storage");

                    // Delete points from vector database
                    await documentDeleter.DeletePointsAsync(message.DocumentId);
                    _logger.LogInformation($"Deleted document {message.DocumentId} from vector database");

                    // Acknowledge the message as successfully processed
                    await Channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation($"Deleted document {message.DocumentId}");
                }
                catch (Exception ex)
                {
                    // Log the error and reject the message, requeueing it for further attempts
                    _logger.LogError(ex, "Failed to process delete event");
                    await Channel.BasicNackAsync(ea.DeliveryTag, false, true);
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