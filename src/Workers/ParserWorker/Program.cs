using ParserWorker;
using ParserWorker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddSingleton<DocumentParser>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
