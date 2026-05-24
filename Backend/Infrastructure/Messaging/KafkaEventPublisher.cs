using System.Text.Json;
using Confluent.Kafka;
using Contracts.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Messaging;

public sealed class KafkaEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"];
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };
        _logger =  logger;
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishEventAsync<T>(string topic, string key, T message, CancellationToken cancellationToken)
    {
        try
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var result = await _producer.ProduceAsync(topic,
                new Message<string, string> { Key = key, Value = jsonMessage },
                cancellationToken);
            _logger.LogInformation("Published: {Topic} key={Key} [{Partition}@{offset}]", topic, key, result.Partition,
                result.Offset);
        }
        catch (ProduceException<string, string> e)
        {
            _logger.LogError(e, "Error during publishing: {Message}", e.Message);
            throw;
        }
    }
    

    public async ValueTask DisposeAsync()
    {
        if (_producer is IAsyncDisposable producerAsyncDisposable)
            await producerAsyncDisposable.DisposeAsync();
        else
            _producer.Dispose();
    }
}