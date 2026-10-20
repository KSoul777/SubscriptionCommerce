using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Domain.Coupons;

public sealed class Coupon
{
    public const int CodeMaxLength = 50;

    private Coupon()
    {
    }

    public int Id { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public DiscountType Type { get; private set; }

    public decimal Value { get; private set; }

    public Currency? Currency { get; private set; }

    public DateTime? ExpiresAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    public static Coupon Create(
        string code,
        string name,
        DiscountType type,
        decimal value,
        Currency? currency,
        DateTime? expiresAtUtc,
        bool isActive = true) =>
        new()
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name,
            Type = type,
            Value = value,
            Currency = currency,
            ExpiresAtUtc = expiresAtUtc,
            IsActive = isActive
        };

    public bool IsRedeemableAt(DateTime utcNow) => IsActive && (ExpiresAtUtc is null || utcNow < ExpiresAtUtc);
}
