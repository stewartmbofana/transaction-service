using System;
using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka;


namespace Transaction.Ingestor;

public class WeatherForecast
{
  public DateOnly Date { get; set; }

  public int TemperatureC { get; set; }

  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

  public string? Summary { get; set; }
}




public class KafkaConsumerExample
{
  public static void Consume()
  {
    var config = new ConsumerConfig
    {
      BootstrapServers = "localhost:9092",
      GroupId = "your-consumer-group-id", // Unique ID for your consumer group
      AutoOffsetReset = AutoOffsetReset.Earliest, // Start consuming from the earliest available message if no offset is committed
      EnableAutoCommit = true // Automatically commit offsets
    };

    using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    consumer.Subscribe("your-topic-name");

    Console.WriteLine("⏳ Waiting for messages...");
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
      e.Cancel = true; // Prevent the process from terminating immediately
      cts.Cancel();
    };

    try
    {
      while (true)
      {
        var consumeResult = consumer.Consume(cts.Token);
        Console.WriteLine($"📥 Received message: {consumeResult.Message.Value} at {consumeResult.TopicPartitionOffset}");
        // Process the message here
      }
    }
    catch (OperationCanceledException)
    {
      // Consumer loop exited
    }
    catch (ConsumeException e)
    {
      Console.WriteLine($"❌ Error: {e.Error.Reason}");
    }
    finally
    {
      consumer.Close(); // Ensure a clean exit and commit final offsets
    }
  }
}



public class KafkaProducerExample
{
  public static async Task ProduceAsync()
  {
    var config = new ProducerConfig
    {
      BootstrapServers = "localhost:9092" // Address of your Kafka broker
    };

    using var producer =  new ProducerBuilder<Null, string>(config).Build();
    var topicName = "your-topic-name";
    var messageValue = "Hello, Kafka from C#!";

    try
    {
      var deliveryReport = await producer.ProduceAsync(
          topicName,
          new Message<Null, string> { Value = messageValue }
      );

      Console.WriteLine($"✅ Message delivered to {deliveryReport.TopicPartitionOffset}");
    }
    catch (ProduceException<Null, string> e)
    {
      Console.WriteLine($"❌ Delivery failed: {e.Error.Reason}");
    }
    finally
    {
      // Optional: Flush any pending messages before disposing
      producer.Flush(TimeSpan.FromSeconds(10));
    }
  }
}

