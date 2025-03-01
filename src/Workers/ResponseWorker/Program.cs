using RabbitMQ.Client;

using ResponseWorker.Services;
using ResponseWorker;


// Create the host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ connection first
var factory = new ConnectionFactory
{
    HostName = builder.Configuration.GetSection("RABBITMQ")["HOST"]
};
var connection = factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection.Result);

// Add services
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddHostedService<Worker>();

// Build and run the host
var host = builder.Build();
host.Run();
