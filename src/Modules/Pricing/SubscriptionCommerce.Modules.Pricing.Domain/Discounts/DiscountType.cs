namespace SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

// How a discount is calculated. This is independent of where the discount comes from (its source).
public enum DiscountType
{
    Percentage = 1,
    FixedAmount = 2
}
