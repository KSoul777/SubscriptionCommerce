using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.PriceLists;

// Persistence model, not a domain entity: the provider turns rows into validated Discount objects.
internal sealed class PriceListDiscount
{
    public int Id { get; init; }

    public int CustomerId { get; init; }

    public string Name { get; init; } = null!;

    public DiscountType Type { get; init; }

    public decimal Value { get; init; }

    public string? Currency { get; init; }

    public bool IsActive { get; init; }
}
