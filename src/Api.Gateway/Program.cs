var builder = WebApplication.CreateBuilder(args);

// Register YARP and load configuration from appsettings.json "ReverseProxy" section
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Optional: a simple health endpoint on the gateway
app.MapGet("/health", () => Results.Ok(new { status = "Gateway running" }));

// Map the reverse proxy
app.MapReverseProxy();

app.Run();
