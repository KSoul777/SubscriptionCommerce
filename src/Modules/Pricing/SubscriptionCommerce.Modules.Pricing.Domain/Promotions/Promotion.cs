using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Domain.Promotions;

public sealed class Promotion
{
    private Promotion()
    {
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public DiscountType Type { get; private set; }

    public decimal Value { get; private set; }

    public Currency? Currency { get; private set; }

    public DateTime StartsAtUtc { get; private set; }

    public DateTime EndsAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    public static Promotion Create(
        string name,
        DiscountType type,
        decimal value,
        Currency? currency,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        bool isActive = true) =>
        new()
        {
            Name = name,
            Type = type,
            Value = value,
            Currency = currency,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            IsActive = isActive
        };

    public bool HasValidWindow => StartsAtUtc < EndsAtUtc;

    public bool IsEligibleAt(DateTime utcNow) => IsActive && StartsAtUtc <= utcNow && utcNow < EndsAtUtc;
}
