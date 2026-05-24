namespace Contracts.Messaging;

public interface IEventConsumer
{
    Task ConsumeEventAsync(CancellationToken cancellationToken);
}