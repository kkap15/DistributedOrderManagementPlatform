using Contracts.Messaging;

namespace NotificationService.Worker;

public class NotificationWorker : BackgroundService
{
    private readonly IEnumerable<IEventConsumer> _consumers;
    private readonly ILogger<NotificationWorker> _logger;

    public NotificationWorker(IEnumerable<IEventConsumer> consumers, ILogger<NotificationWorker> logger)
    {
        _consumers = consumers;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = _consumers.Select(consumer =>
            Task.Run(async () =>
            {
                try
                {
                    await consumer.ConsumeEventAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Consumer {type} failed:", consumer.GetType().Name);
                }
            }, stoppingToken));

        await Task.WhenAll(tasks);
    }
}