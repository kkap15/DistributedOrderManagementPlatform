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
                    _consumer.Subscribe(_topic);
                    var cr = _consumer.Consume(cancellationToken);
                    var data = JsonSerializer.Deserialize<TEvent>(cr.Message.Value)!;
                    await HandleAsync(data, cancellationToken);
                    _consumer.Commit(cr);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    logger.LogError(e, "Consume error: {Reason}", e.Error.Reason);
                    await Task.Delay(2000, cancellationToken);
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