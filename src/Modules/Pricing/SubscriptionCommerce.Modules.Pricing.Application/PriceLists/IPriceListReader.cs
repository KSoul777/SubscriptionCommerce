using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.PriceLists;

public sealed record PriceListDiscountRecord(
    int Id,
    string Name,
    DiscountType Type,
    decimal Value,
    string? Currency,
    bool IsActive);

public interface IPriceListReader
{
    Task<IReadOnlyList<PriceListDiscountRecord>> GetForCustomerAsync(int customerId, CancellationToken cancellationToken);
}
