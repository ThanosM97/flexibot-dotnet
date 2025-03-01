using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Api.Hubs;
using Shared.Events;
using Shared.Services;


namespace Api.Services
{
    /// <summary>
    /// Handles consuming messages from RabbitMQ for chat response streaming.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context.</param>
    /// <param name="connection">The RabbitMQ connection.</param>
    /// <param name="logger">The logger instance.</param>
    public class ChatResponseStreamConsumer(
        IHubContext<ChatHub> hubContext,
        IConnection connection,
        ILogger<ChatResponseStreamConsumer> logger
    ) : RabbitMQConsumerBase<ChatResponseStreamedEvent>(connection, "chat_response_streamed", "chat")
    {
        private readonly IHubContext<ChatHub> _hubContext = hubContext;
        private readonly ILogger<ChatResponseStreamConsumer> _logger = logger;

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
                try
                {
                    // Get message from queue
                    var message = DeserializeEvent(ea.Body.ToArray());

                    if (message != null)
                    {
                        // Send the token chunk to the SignalR group identified by the JobId
                        await _hubContext.Clients.Group(message.JobId)
                            .SendAsync("ReceiveToken", message.Chunk, message.Done);
                        _logger.LogInformation($"Sent token chunk to {message.JobId}");
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed.
                    _logger.LogError($"Error processing message: {ex.Message}");
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


    /// <summary>
    /// Background service for managing the lifecycle of the chat response stream consumer.
    /// </summary>
    /// <param name="consumer">The chat response stream consumer.</param>
    /// <param name="logger">The logger instance.</param>
    public class ChatResponseStreamBackgroundService(
        ChatResponseStreamConsumer consumer, ILogger<ChatResponseStreamBackgroundService> logger
    ) : BackgroundService
    {
        private readonly ChatResponseStreamConsumer _consumer = consumer;
        private readonly ILogger<ChatResponseStreamBackgroundService> _logger = logger;

        /// <summary>
        /// Executes the background service asynchronously.
        /// </summary>
        /// <param name="stoppingToken">A token to signal the request to stop.</param>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting RabbitMQ consumer");
                await _consumer.StartConsumingAsync();
                _logger.LogInformation("RabbitMQ consumer started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start RabbitMQ consumer");
            }
        }
    }
}
