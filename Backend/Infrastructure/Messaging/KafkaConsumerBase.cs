using System.Text.Json;
using Confluent.Kafka;
using Contracts.Messaging;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging;

public abstract class KafkaConsumerBase<TEvent> : IEventConsumer
{
    protected readonly ILogger logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly string _topic;

    protected KafkaConsumerBase(string bootstrapServers, string groupId, string topic, ILogger logger)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _topic = topic;
        this.logger = logger;
    }

    public async Task ConsumeEventAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting consumer for topic: {Topic}", _topic);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _consumer.Subscribe(_topic);
                logger.LogInformation("Subscribed to: {Topic}", _topic);
                break;
            }
            catch (Exception e)
            {
                logger.LogWarning("Waiting for topic {Topic}: {Error}",  _topic, e.Message);
                await Task.Delay(3000, cancellationToken);
            } 
        }
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _consumer.Consume(cancellationToken);
                    if (cr?.Message?.Value is null || string.IsNullOrWhiteSpace(cr.Message.Value))
                        continue;
                    var raw = cr.Message.Value;
                    if (raw.StartsWith("\"") && raw.EndsWith("\""))
                    {
                        raw = JsonSerializer.Deserialize<string>(raw)!;
                    }

                    var data = JsonSerializer.Deserialize<TEvent>(raw)!;
                    await HandleAsync(data, cancellationToken);
                    _consumer.Commit(cr);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    if (e.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        logger.LogWarning("Topic {Topic} not found. Waiting for topic creation", _topic);
                        await Task.Delay(5000, cancellationToken);
                    }
                    else
                    {
                        logger.LogError(e, "Consume error: {Reason}", e.Error.Reason);
                        await Task.Delay(2000, cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unhandled error processing message on topic {Topic}", _topic);
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }

    protected abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}