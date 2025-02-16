using ParserWorker;
using ParserWorker.Services;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ connection first
var factory = new ConnectionFactory
{
    HostName = builder.Configuration.GetSection("RABBITMQ")["HOST"]
};
var connection = factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection.Result);

// Add services
builder.Services.AddSingleton<DocumentParser>();
builder.Services.AddSingleton<MinioService>();
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
