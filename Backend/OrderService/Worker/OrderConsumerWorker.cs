using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OrderService.Worker;

public class OrderConsumerWorker : BackgroundService
{
    private readonly IEventConsumer _eventConsumer;
    private readonly ILogger<OrderConsumerWorker> _logger;
    
    public OrderConsumerWorker(IEventConsumer eventConsumer, ILogger<OrderConsumerWorker> logger)
    {
        _eventConsumer = eventConsumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _eventConsumer.ConsumeEventAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Event consumer worker failed: {Message}", ex.Message);
        }
    }
}