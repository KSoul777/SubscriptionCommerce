using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Promotions;

internal sealed class Promotion
{
    public int Id { get; init; }

    public string Name { get; init; } = null!;

    public DiscountType Type { get; init; }

    public decimal Value { get; init; }

    public string? Currency { get; init; }

    public DateTime StartsAtUtc { get; init; }

    public DateTime EndsAtUtc { get; init; }

    public bool IsActive { get; init; }
}
