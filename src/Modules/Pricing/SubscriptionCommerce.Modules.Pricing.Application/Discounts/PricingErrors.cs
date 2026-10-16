using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Application.Discounts;

public static class PricingErrors
{
    // Stored discount data that breaks the domain rules is a server-side data problem, not a client error.
    public static Error InvalidStoredDiscount(string source, int id, Error reason) =>
        Error.Failure(
            "Pricing.InvalidStoredDiscount",
            $"The {source} discount with id {id} is invalid: {reason.Description}");
}
