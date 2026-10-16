namespace SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;

// Another request wrote first: either the row version was stale or a unique key was taken
// (for example two requests creating the same customer's first value at once). Callers map it to 409.
public sealed class ConcurrencyConflictException(string message, Exception innerException)
    : Exception(message, innerException);
