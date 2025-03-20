using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

using Shared.Interfaces.Database;
using Shared.Models;
using Shared.Services.Database;
using StatusWorker;
using StatusWorker.Services;


var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ connection first
var factory = new ConnectionFactory
{
    HostName = builder.Configuration.GetSection("RABBITMQ")["HOST"]
};
var connection = factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection.Result);

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration["ConnectionStrings:Postgres"]));

// Add services
builder.Services.AddScoped<IDatabaseService<DocumentMetadata>, PostgresRepository<DocumentMetadata>>();
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddSingleton<StatusUpdater>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
