using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

using Shared.Events;
using Shared.Services;

namespace ResponseWorker.Services
{
    /// <summary>
    /// RabbitMQ consumer for handling chat prompted events.
    /// </summary>
    /// <param name="connection">The RabbitMQ connection.</param>
    /// <param name="logger">The logger instance.</param>
    public class RabbitMQConsumer(
        IConnection connection,
        ILogger<RabbitMQConsumer> logger) : RabbitMQConsumerBase<ChatPromptedEvent>(connection, "chat_prompted", "chat")
    {
        private readonly ILogger<RabbitMQConsumer> _logger = logger;

        /// <summary>
        /// Starts consuming messages asynchronously from the RabbitMQ queue.
        /// </summary>
        public override async Task StartConsumingAsync()
        {
            // Create a new asynchronous eventing basic consumer for the channel
            var consumer = new AsyncEventingBasicConsumer(Channel);

            // Register the ReceivedAsync event handler
            consumer.ReceivedAsync += async (model, ea) =>
            {
                // Get message from queue
                var message = DeserializeEvent(ea.Body.ToArray());

                try
                {
                    _logger.LogInformation($"Received chat: {message.JobId}");
                    _logger.LogInformation($"{message.Prompt}");

                    // TODO: Implement the chat response generation logic
                    // foreach (var chunk in chunks)
                    // {
                    //     var body = JsonSerializer.SerializeToUtf8Bytes(chunk);
                    //     await Channel.BasicPublishAsync(
                    //         exchange: "chat",
                    //         routingKey: "chat_response_streamed",
                    //         body: body
                    //     );
                    // }

                    await Channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation($"Streamed response for chat: {message.JobId}");

                }
                catch (Exception ex)
                {
                    // Log the exception
                    _logger.LogError(ex, "Error processing chat");
                    await Channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            // Start consuming messages from the queue
            await Channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );
        }
    }
}