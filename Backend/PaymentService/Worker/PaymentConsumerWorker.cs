using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaymentService.Worker;

public class PaymentConsumerWorker : BackgroundService
{
    private readonly IEventConsumer _eventConsumer;
    private readonly ILogger<PaymentConsumerWorker> _logger;

    public PaymentConsumerWorker(IEventConsumer eventConsumer, ILogger<PaymentConsumerWorker> logger)
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
        };
    }
}