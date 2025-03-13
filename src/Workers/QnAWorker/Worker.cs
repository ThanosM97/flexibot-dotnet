using QnAWorker.Services;

namespace QnAWorker;

public class Worker(
    RabbitMQConsumerUpload consumerUpload, RabbitMQConsumerDelete consumerDelete, ILogger<Worker> logger
) : BackgroundService
{
    private readonly RabbitMQConsumerUpload _consumerUpload = consumerUpload;
    private readonly RabbitMQConsumerDelete _consumerDelete = consumerDelete;
    private readonly ILogger<Worker> _logger = logger;

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting RabbitMQ consumers");
            await _consumerUpload.StartConsumingAsync();
            await _consumerDelete.StartConsumingAsync();
            _logger.LogInformation("RabbitMQ consumers started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ consumers");
        }
    }
}