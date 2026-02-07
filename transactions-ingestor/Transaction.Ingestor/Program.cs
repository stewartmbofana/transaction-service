using Microsoft.EntityFrameworkCore;
using Transaction.Ingestor.Data;
using Transaction.Ingestor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register DbContext
builder.Services.AddDbContext<TransactionDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register categorizer (reads configuration)
builder.Services.AddSingleton<ITransactionCategorizer, TransactionCategorizer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Initial database migration and update
using (var scope = app.Services.CreateScope()) {
  var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
  dbContext.Database.Migrate();
}
