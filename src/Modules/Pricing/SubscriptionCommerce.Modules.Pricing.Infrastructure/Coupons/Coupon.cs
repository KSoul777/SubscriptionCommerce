using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Coupons;

internal sealed class Coupon
{
    public int Id { get; init; }

    public string Code { get; init; } = null!;

    public string Name { get; init; } = null!;

    public DiscountType Type { get; init; }

    public decimal Value { get; init; }

    public string? Currency { get; init; }

    public DateTime? ExpiresAtUtc { get; init; }

    public bool IsActive { get; init; }
}
