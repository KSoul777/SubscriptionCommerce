using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Pricing.Application.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.PriceLists;

internal sealed class PriceListReader(PricingDbContext dbContext) : IPriceListReader
{
    public async Task<IReadOnlyList<PriceListDiscountRecord>> GetForCustomerAsync(
        int customerId,
        CancellationToken cancellationToken) =>
        await dbContext.PriceListDiscounts
            .AsNoTracking()
            .Where(d => d.CustomerId == customerId)
            .Select(d => new PriceListDiscountRecord(d.Id, d.Name, d.Type, d.Value, d.Currency, d.IsActive))
            .ToListAsync(cancellationToken);
}
