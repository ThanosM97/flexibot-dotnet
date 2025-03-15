using RabbitMQ.Client;

using ResponseWorker;
using ResponseWorker.Services;
using Shared.Interfaces.Storage;
using Shared.Services.Storage;


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
builder.Services.AddSingleton<ChatBot>();
builder.Services.AddSingleton<IStorageService, MinioService>();
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddHostedService<Worker>();

// Build and run the host
var host = builder.Build();
host.Run();
