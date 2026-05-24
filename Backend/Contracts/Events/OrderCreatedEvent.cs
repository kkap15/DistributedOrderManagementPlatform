namespace Contracts.Events;

public sealed record OrderCreatedEvent(
    string OrderId,
    string UserId,
    decimal Amount,
    DateTime CreatedAt
    );