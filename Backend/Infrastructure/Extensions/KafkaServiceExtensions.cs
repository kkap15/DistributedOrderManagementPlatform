using Contracts.Messaging;
using Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class KafkaServiceExtensions
{
    public static IServiceCollection AddKafkaPublisher(this IServiceCollection services)
    {
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        
        return services;
    }
} 