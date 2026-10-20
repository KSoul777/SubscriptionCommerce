using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Domain.PriceLists;

public sealed class PriceListDiscount
{
    private PriceListDiscount()
    {
    }

    public int Id { get; private set; }

    public int CustomerId { get; private set; }

    public string Name { get; private set; } = null!;

    public DiscountType Type { get; private set; }

    public decimal Value { get; private set; }

    public Currency? Currency { get; private set; }

    public bool IsActive { get; private set; }

    public static PriceListDiscount Create(
        int customerId,
        string name,
        DiscountType type,
        decimal value,
        Currency? currency,
        bool isActive = true) =>
        new()
        {
            CustomerId = customerId,
            Name = name,
            Type = type,
            Value = value,
            Currency = currency,
            IsActive = isActive
        };

    public bool IsEligibleFor(int customerId) => IsActive && CustomerId == customerId;
}
