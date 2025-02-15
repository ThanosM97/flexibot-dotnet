using EmbedderWorker;
using EmbedderWorker.Services;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ connection first
var factory = new ConnectionFactory
{
    HostName = builder.Configuration.GetSection("RABBITMQ")["HOST"]
};
var connection = factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection);

// Add services
builder.Services.AddSingleton<DocumentEmbedder>();
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
