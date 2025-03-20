using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using StackExchange.Redis;

using ResponseWorker;
using ResponseWorker.Services;
using Shared.Interfaces.Cache;
using Shared.Interfaces.Database;
using Shared.Interfaces.Storage;
using Shared.Models;
using Shared.Services.Cache;
using Shared.Services.Database;
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

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Add services
builder.Services.AddSingleton<ChatBot>();
builder.Services.AddSingleton<IStorageService, MinioService>();
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddSingleton<ICacheService, RedisService>();
builder.Services.AddScoped<IDatabaseService<ChatLog>, PostgresRepository<ChatLog>>();
builder.Services.AddHostedService<Worker>();

// Build and run the host
var host = builder.Build();
host.Run();
