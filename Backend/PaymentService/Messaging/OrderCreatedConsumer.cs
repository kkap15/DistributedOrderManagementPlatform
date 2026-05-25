using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Events;
using Contracts.Messaging;
using Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentService.Models;
using PaymentService.Repositories;

namespace PaymentService.Messaging;

public class OrderCreatedConsumer : KafkaConsumerBase<OrderCreatedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventPublisher _eventPublisher;
    
    public OrderCreatedConsumer(IConfiguration configuration, ILogger<OrderCreatedConsumer> logger, 
        IServiceScopeFactory scopeFactory, IEventPublisher eventPublisher)
        : base(configuration["Kafka:BootstrapServers"]!, "payment-service", Topics.OrderCreated, logger)
    {
        _scopeFactory = scopeFactory;
        _eventPublisher = eventPublisher;
    }
        
    protected override async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope() ;
        var paymentRepositories = scope.ServiceProvider.GetRequiredService<IPaymentRepositories>();
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TransactionId = Guid.NewGuid(),
            ProcessedAt = DateTime.UtcNow,
            Status = "PaymentProcessed",
            OrderId = @event.OrderId,
        };
        
        await paymentRepositories.AddPaymentAsync(payment);
        await paymentRepositories.SaveAsync();

        var paymentProcessedEvent = new PaymentProcessedEvent
        (
            OrderId: @event.OrderId,
            PaymentId: payment.Id.ToString(),
            FailureReason: null,
            ProcessedAt: new DateTimeOffset(payment.ProcessedAt, TimeSpan.Zero),
            Success: true
        );

        await _eventPublisher.PublishEventAsync(Topics.PaymentProcessed, @event.OrderId, paymentProcessedEvent,
            cancellationToken);

        logger.LogInformation("PaymentId {PaymentId} status updated to {Status}", paymentProcessedEvent.PaymentId,
            payment.Status);
    }
}