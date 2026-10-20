namespace SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;

public sealed class ConcurrencyConflictException(ConflictKind kind, string message, Exception innerException)
    : Exception(message, innerException)
{
    public ConflictKind Kind { get; } = kind;
}
