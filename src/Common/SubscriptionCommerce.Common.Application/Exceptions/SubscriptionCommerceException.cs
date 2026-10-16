using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Common.Application.Exceptions;

public sealed class SubscriptionCommerceException(string requestName, Error? error = default, Exception? innerException = default)
    : Exception("Application exception", innerException)
{
    public string RequestName { get; } = requestName;

    public Error? Error { get; } = error;
}
