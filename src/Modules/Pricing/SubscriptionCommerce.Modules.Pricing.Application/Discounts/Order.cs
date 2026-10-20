using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Application.Discounts;

public sealed record Order(int CustomerId, Money Price, IReadOnlyCollection<string> CouponCodes);
