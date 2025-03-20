using RabbitMQ.Client;
using StackExchange.Redis;

using ResponseWorker;
using ResponseWorker.Services;
using Shared.Interfaces.Cache;
using Shared.Interfaces.Storage;
using Shared.Services.Cache;
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

// Configure Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("REDIS")));

// Add services
builder.Services.AddSingleton<ChatBot>();
builder.Services.AddSingleton<IStorageService, MinioService>();
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddSingleton<ICacheService, RedisService>();
builder.Services.AddHostedService<Worker>();

// Build and run the host
var host = builder.Build();
host.Run();
