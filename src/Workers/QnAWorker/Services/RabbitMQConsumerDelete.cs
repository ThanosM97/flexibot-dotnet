using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Shared.Events;
using Shared.Services;

namespace QnAWorker.Services
{
    /// <summary>
    /// Consumes messages from a RabbitMQ queue and clears the QnA knowledge base.
    /// </summary>
    /// <remarks>
    /// The class uses asynchronous operations for communication with RabbitMQ, clearing the vector database,
    /// and removing the file from storage.
    /// It declares the necessary queue on the established RabbitMQ connection, and starts
    /// consuming messages when <see cref="StartConsumingAsync"/> is invoked.
    /// </remarks>
    public class RabbitMQConsumerDelete(
        IConnection connection,
        QnAProcessor qnAProcessor,
        ILogger<RabbitMQConsumerDelete> logger) : RabbitMQConsumerBase<QnADeletedEvent>(connection, "qna_deleted")
    {
        private readonly QnAProcessor _qnAProcessor = qnAProcessor;
        private readonly ILogger<RabbitMQConsumerDelete> _logger = logger;

        public override async Task StartConsumingAsync()
        {
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                // Get message from queue
                var message = DeserializeEvent(ea.Body.ToArray());

                try
                {
                    _logger.LogInformation($"Deleting document: {message.FileName}");

                    // Clear QnA knowledge base
                    await _qnAProcessor.ClearQnAKnowledgeBase(message.FileName);

                    await Channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation($"Deleted {message.FileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting document");
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