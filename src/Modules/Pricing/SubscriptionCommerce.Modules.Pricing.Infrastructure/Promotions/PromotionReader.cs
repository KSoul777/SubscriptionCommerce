using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Pricing.Application.Promotions;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Promotions;

internal sealed class PromotionReader(PricingDbContext dbContext) : IPromotionReader
{
    public async Task<IReadOnlyList<PromotionRecord>> GetPromotionsAsync(CancellationToken cancellationToken) =>
        await dbContext.Promotions
            .AsNoTracking()
            .Select(p => new PromotionRecord(
                p.Id,
                p.Name,
                p.Type,
                p.Value,
                p.Currency,
                p.StartsAtUtc,
                p.EndsAtUtc,
                p.IsActive))
            .ToListAsync(cancellationToken);
}
