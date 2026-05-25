using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Repositories;

namespace OrderService.Worker;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OutboxProcessor> _logger;
    
    public OutboxProcessor(IServiceScopeFactory scopeFactory, IEventPublisher eventPublisher, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IOrderRepositories>();

                var unpublishedEvents = await repository.GetUnpublishedOutboxMessagesAsync();
                
                if (unpublishedEvents.Count == 0) continue;
                _logger.LogInformation("Publishing {Count} unpublished events", unpublishedEvents.Count);

                foreach (var unpublishedEvent in unpublishedEvents)
                {
                    try
                    {
                        await _eventPublisher.PublishRawEventAsync(unpublishedEvent.Topic,
                            unpublishedEvent.Id.ToString(),
                            unpublishedEvent.Payload, stoppingToken);
                        unpublishedEvent.IsPublished = true;
                        await repository.SaveAsync();
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        _logger.LogError(e, "Failed to publish event {Id}", unpublishedEvent.Id);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}