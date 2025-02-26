using RabbitMQ.Client;

using IndexerWorker;
using IndexerWorker.Services;
using Shared.Factories.Search;
using Shared.Interfaces.Search;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ connection first
var factory = new ConnectionFactory
{
    HostName = builder.Configuration.GetSection("RABBITMQ")["HOST"]
};
var connection = factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection.Result);

// Add services
builder.Services.AddSingleton<DocumentIndexer>();
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddSingleton<IVectorDatabaseService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return VectorDatabaseFactory.GetVectorDatabaseService(config);
});
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
