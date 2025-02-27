using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

using DeleterWorker;
using DeleterWorker.Services;
using Shared.Factories.Search;
using Shared.Interfaces.Database;
using Shared.Interfaces.Search;
using Shared.Services.Database;


var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ connection first
var factory = new ConnectionFactory
{
    HostName = builder.Configuration.GetSection("RABBITMQ")["HOST"]
};
var connection = factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection.Result);

// Add database context
builder.Services.AddDbContext<DocumentDbContext>(options =>
    options.UseNpgsql(builder.Configuration["ConnectionStrings:Postgres"]));

// Add services
builder.Services.AddScoped<IDocumentRepository, PostgresRepository>();
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddSingleton<IVectorDatabaseService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return VectorDatabaseFactory.GetVectorDatabaseService(config);
});
builder.Services.AddSingleton<DocumentDeleter>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
