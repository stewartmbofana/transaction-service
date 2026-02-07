using Confluent.Kafka;
using System.Text.Json;
using Transaction.Shared.Models;

namespace Transaction.Worker;

	public class Worker(ILogger<Worker> logger) : BackgroundService
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
			consumer.Subscribe("TransactionsTopic");

			while (!stoppingToken.IsCancellationRequested)
			{
				var consumedData = consumer.Consume(TimeSpan.FromSeconds(3));
				if (consumedData is not null)
				{

					var transaction = JsonSerializer.Deserialize<BankTransaction>(consumedData.Message.Value);
					logger.LogInformation($"Consuming {transaction}");


					//EmployeeReport er = new(Guid.NewGuid(), employee.Id, employee.Name, employee.Surname);
					//_reportDbContext.Reports.Add(er);

					//await _reportDbContext.SaveChangesAsync();
				}
				else
				{
					logger.LogInformation("Nothing found to consume");
				}
				// await Task.Delay(1000, stoppingToken);
			}
		}
	}
