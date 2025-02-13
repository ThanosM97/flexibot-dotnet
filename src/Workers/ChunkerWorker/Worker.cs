using ChunkerWorker.Services;

namespace ChunkerWorker;

public class Worker(RabbitMQConsumer consumer, ILogger<Worker> logger)
    : BackgroundService
{
    private readonly RabbitMQConsumer _consumer = consumer;
    private readonly ILogger<Worker> _logger = logger;

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