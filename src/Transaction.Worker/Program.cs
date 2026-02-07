using Transaction.Worker;
using Transaction.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// Register categorizer (reads configuration)
builder.Services.AddSingleton<ITransactionCategorizer, TransactionCategorizer>();

var host = builder.Build();
host.Run();
