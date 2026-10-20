using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Application.Discounts;

public static class PricingErrors
{
    public static Error InvalidStoredDiscount(string source, int id, Error reason) =>
        Error.Failure(
            "Pricing.InvalidStoredDiscount",
            $"The {source} discount with id {id} is invalid: {reason.Description}");
}
