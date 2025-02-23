using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

using Shared.Interfaces.Database;
using Shared.Interfaces.Storage;
using Shared.Services.Database;
using Shared.Services.Storage;
using Shared.Services;


var builder = WebApplication.CreateBuilder(args);

// Add logging services
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add MVC services
builder.Services.AddControllers();

// Add database context
builder.Services.AddDbContext<DocumentDbContext>(options =>
    options.UseNpgsql(builder.Configuration["ConnectionStrings:Postgres"]));

// Add RabbitMQ connection
var factory = new ConnectionFactory
{
    HostName = builder.Configuration.GetSection("RABBITMQ")["HOST"]
};
var connection = factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection.Result);

// Add services
builder.Services.AddSingleton<IStorageService, MinioService>();
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddScoped<IDocumentRepository, PostgresRepository>();

var app = builder.Build();

// Ensure RabbitMQPublisher is initialized at startup
var publisher = app.Services.GetRequiredService<RabbitMQPublisher>();

// Map controllers to routes
app.MapControllers();

app.Run();