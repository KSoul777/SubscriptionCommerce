using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Pricing.Application.Discounts.CalculateOrderDiscounts;

public sealed record CalculateOrderDiscountsQuery(
    int CustomerId,
    decimal Price,
    string Currency,
    IReadOnlyCollection<string> CouponCodes) : IQuery<DiscountBreakdownResponse>;
