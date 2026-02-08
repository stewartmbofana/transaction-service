using Confluent.Kafka;
using Library.Shared.Data;
using Library.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Transactions.Worker.Services;

namespace Transactions.Worker;

	public class Worker(
		TransactionsDbContext context,
        ITransactionCategorizer categorizer,
        ILogger<Worker> logger) : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var consumerConfig = new ConsumerConfig()
			{
				BootstrapServers = "localhost:9092",
				GroupId = "TransactionsConsumerGroup1",
				ClientId = Guid.NewGuid().ToString(),
				AutoOffsetReset = AutoOffsetReset.Earliest,
				EnableAutoCommit = true
			};

			using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
			consumer.Subscribe("bank.transactions");

			while (!stoppingToken.IsCancellationRequested)
			{
				var consumedData = consumer.Consume(TimeSpan.FromSeconds(3));
				if (consumedData is not null)
				{

					var transaction = JsonSerializer.Deserialize<BankTransaction>(consumedData.Message.Value);
					logger.LogInformation($"Consuming {transaction}");


				// Determine category name using intelligent, configuration-driven categorizer
				var categoryName = categorizer.GetCategoryName(transaction.Description ?? string.Empty);

				// Try find existing category, otherwise create it
				var category = await context.Categories
					.FirstOrDefaultAsync(c => c.Name == categoryName);

				if (category == null)
				{
					category = new Category { Name = categoryName };
					context.Categories.Add(category);

					await context.SaveChangesAsync(); // ensure Category.Id is set
				}

				transaction.CategoryId = category.Id;

				context.Transactions.Add(transaction);

				await context.SaveChangesAsync();

				logger.LogInformation($"Transaction {transaction} saved with category {categoryName}");
			}
				else
				{
					logger.LogInformation("Nothing found to consume");
				}
				// await Task.Delay(1000, stoppingToken);
			}
		}
	}
