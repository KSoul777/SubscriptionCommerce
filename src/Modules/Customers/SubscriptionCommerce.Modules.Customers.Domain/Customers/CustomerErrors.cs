using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Customers.Domain.Customers;

public static class CustomerErrors
{
    public static Error NotFound(int customerId) =>
        Error.NotFound("Customers.NotFound", $"The customer with the identifier {customerId} was not found");

    public static readonly Error NameInvalid = Error.Problem(
        "Customers.NameInvalid",
        $"The customer name is required and must be at most {Customer.NameMaxLength} characters");
}
