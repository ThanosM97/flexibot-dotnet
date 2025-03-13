using QnAWorker;
using QnAWorker.Services;
using RabbitMQ.Client;
using Shared.Interfaces.Storage;
using Shared.Services.Storage;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ connection first
var factory = new ConnectionFactory
{
    HostName = builder.Configuration.GetSection("RABBITMQ")["HOST"]
};
var connection = factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection.Result);

// Add services
builder.Services.AddSingleton<QnAProcessor>();
builder.Services.AddSingleton<IStorageService, MinioService>();
builder.Services.AddSingleton<RabbitMQConsumerUpload>();
builder.Services.AddSingleton<RabbitMQConsumerDelete>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
