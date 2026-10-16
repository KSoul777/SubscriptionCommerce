using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Application.Discounts;

// What a discount provider needs to know about the order. CouponCodes are the codes the customer entered,
// trimmed, upper-cased and without duplicates.
public sealed record Order(int CustomerId, Money Price, IReadOnlyCollection<string> CouponCodes);
