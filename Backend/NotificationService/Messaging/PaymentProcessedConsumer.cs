using Contracts;
using Contracts.Events;
using Infrastructure.Messaging;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Messaging;

public class PaymentProcessedConsumer : KafkaConsumerBase<PaymentProcessedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<PaymentProcessedConsumer> _logger;
    
    public PaymentProcessedConsumer(IConfiguration configuration, IHubContext<NotificationHub> hubContext, ILogger<PaymentProcessedConsumer> logger) 
        : base(configuration["Kafka:BootstrapServers"]!, "notification-service-v2", Topics.PaymentProcessed, logger)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task HandleAsync(PaymentProcessedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending PaymentProcessed notification for order {OrderId}", @event.OrderId);
        await _hubContext.Clients.All.SendAsync("PaymentProcessed", @event, cancellationToken);
        _logger.LogInformation("PaymentProcessed notification sent for order {OrderId}", @event.OrderId);
    }
}