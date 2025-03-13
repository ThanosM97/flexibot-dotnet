using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Shared.Events;
using Shared.Services;

namespace QnAWorker.Services
{
    /// <summary>
    /// Consumes messages from a RabbitMQ queue, processes QnA csv by downloading and parsing it,
    /// and uploads the points to the vector database.
    /// </summary>
    /// <remarks>
    /// The class uses asynchronous operations for communication with RabbitMQ, and csv processing.
    /// It declares the necessary queue on the established RabbitMQ connection, and starts
    /// consuming messages when <see cref="StartConsumingAsync"/> is invoked.
    /// </remarks>
    public class RabbitMQConsumerUpload(
        IConnection connection,
        QnAProcessor qnAProcessor,
        ILogger<RabbitMQConsumerUpload> logger) : RabbitMQConsumerBase<QnAUploadedEvent>(connection, "qna_uploaded")
    {
        private readonly QnAProcessor _qnAProcessor = qnAProcessor;
        private readonly ILogger<RabbitMQConsumerUpload> _logger = logger;

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

                    // Process qna csv file and upload points to vector db
                    var textContent = _qnAProcessor.ProcessUploadAsync(message.FileName);

                    await Channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation($"Uploaded {message.FileName}");
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
    }
}