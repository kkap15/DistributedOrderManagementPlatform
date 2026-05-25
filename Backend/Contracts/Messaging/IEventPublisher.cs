namespace Contracts.Messaging;

public interface IEventPublisher
{
    Task PublishEventAsync<T>(string topic, string key, T message, CancellationToken cancellationToken);
    Task PublishRawEventAsync(string topic, string key, string payload, CancellationToken cancellationToken);
}