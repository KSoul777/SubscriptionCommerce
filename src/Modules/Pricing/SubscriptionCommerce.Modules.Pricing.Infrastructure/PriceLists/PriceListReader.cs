using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Pricing.Application.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Domain.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.PriceLists;

internal sealed class PriceListReader(PricingDbContext dbContext) : IPriceListReader
{
    public async Task<IReadOnlyList<PriceListDiscount>> GetActiveForCustomerAsync(
        int customerId,
        CancellationToken cancellationToken) =>
        await dbContext.PriceListDiscounts
            .AsNoTracking()
            .Where(d => d.CustomerId == customerId && d.IsActive)
            .ToListAsync(cancellationToken);
}
