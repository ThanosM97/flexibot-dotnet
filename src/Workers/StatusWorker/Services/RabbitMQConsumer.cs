using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Shared.Events;
using Shared.Services;

namespace StatusWorker.Services
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
        StatusUpdater statusUpdater,
        ILogger<RabbitMQConsumer> logger
    ) : RabbitMQConsumerBase<DocumentStatusEvent>(connection, "document_status")
    {
        public override async Task StartConsumingAsync()
        {
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var message = DeserializeEvent(ea.Body.ToArray());
                    logger.LogInformation($"Processing status event: {message.DocumentId}");

                    await statusUpdater.UpdateDocumentStatusAsync(message.DocumentId, message.EventStatus);

                    await Channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process status event");
                    await Channel.BasicNackAsync(ea.DeliveryTag, false, true);
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