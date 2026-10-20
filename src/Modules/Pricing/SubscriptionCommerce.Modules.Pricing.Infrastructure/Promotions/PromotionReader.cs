using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Pricing.Application.Promotions;
using SubscriptionCommerce.Modules.Pricing.Domain.Promotions;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Promotions;

internal sealed class PromotionReader(PricingDbContext dbContext) : IPromotionReader
{
    public async Task<IReadOnlyList<Promotion>> GetActiveNotEndedAsync(DateTime utcNow, CancellationToken cancellationToken) =>
        await dbContext.Promotions
            .AsNoTracking()
            .Where(p => p.IsActive && p.EndsAtUtc > utcNow)
            .ToListAsync(cancellationToken);
}
