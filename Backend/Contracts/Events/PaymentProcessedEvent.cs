namespace Contracts.Events;

public sealed record PaymentProcessedEvent(
    string OrderId,
    string PaymentId,
    bool Success,
    string? FailureReason,
    DateTimeOffset ProcessedAt);