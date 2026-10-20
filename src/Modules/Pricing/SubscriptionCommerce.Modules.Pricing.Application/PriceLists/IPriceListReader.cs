using SubscriptionCommerce.Modules.Pricing.Domain.PriceLists;

namespace SubscriptionCommerce.Modules.Pricing.Application.PriceLists;

public interface IPriceListReader
{
    Task<IReadOnlyList<PriceListDiscount>> GetActiveForCustomerAsync(int customerId, CancellationToken cancellationToken);
}
