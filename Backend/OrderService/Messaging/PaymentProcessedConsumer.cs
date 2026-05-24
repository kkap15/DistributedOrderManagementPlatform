using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Events;
using Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Repositories;

namespace OrderService.Messaging;

public class PaymentProcessedConsumer : KafkaConsumerBase<PaymentProcessedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PaymentProcessedConsumer(IConfiguration configuration, ILogger<PaymentProcessedConsumer> logger, IServiceScopeFactory scopeFactory)
        : base(configuration["Kafka:BootstrapServers"]!, "order-service", Topics.PaymentProcessed, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task HandleAsync(PaymentProcessedEvent @event, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope() ;
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepositories>();
        var order = await orderRepository.GetOrderByIdAsync(Guid.Parse(@event.OrderId));

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found",  @event.OrderId);
            return;
        }

        order.Status = @event.Success ? "Paid" : "Failed";

        await orderRepository.SaveAsync();

        logger.LogInformation("Order {OrderId} status updated to {Status}", @event.OrderId, order.Status);
    }
}