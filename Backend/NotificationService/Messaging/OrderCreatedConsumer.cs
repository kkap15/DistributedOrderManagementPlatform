using Contracts;
using Contracts.Events;
using Infrastructure.Messaging;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Messaging;

public class OrderCreatedConsumer : KafkaConsumerBase<OrderCreatedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    
    public OrderCreatedConsumer(IConfiguration configuration, ILogger<OrderCreatedConsumer> logger, IHubContext<NotificationHub> hubContext) 
        : base(configuration["Kafka:BootstrapServers"]!, "notification-service-v2", Topics.OrderCreated, logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending OrderCreatedEvent notification for order: {OrderId}", @event.OrderId);
        await _hubContext.Clients.All.SendAsync("OrderCreated", @event, cancellationToken);
        _logger.LogInformation("OrderCreated notification sent for order {OrderId}", @event.OrderId);
    }
}