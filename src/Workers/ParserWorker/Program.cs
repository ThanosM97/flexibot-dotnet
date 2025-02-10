using ParserWorker;
using ParserWorker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddSingleton<DocumentParser>();
builder.Services.AddSingleton<MinioService>();
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();


// Start RabbitMQ consumer when host starts
var rabbitConsumer = host.Services.GetRequiredService<RabbitMQConsumer>();
await rabbitConsumer.StartConsumingAsync();

host.Run();
