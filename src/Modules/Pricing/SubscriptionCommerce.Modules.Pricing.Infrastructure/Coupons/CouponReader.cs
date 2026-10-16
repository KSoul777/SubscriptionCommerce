using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Pricing.Application.Coupons;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Coupons;

internal sealed class CouponReader(PricingDbContext dbContext) : ICouponReader
{
    public async Task<IReadOnlyList<CouponRecord>> GetByCodesAsync(
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken) =>
        await dbContext.Coupons
            .AsNoTracking()
            .Where(c => codes.Contains(c.Code))
            .Select(c => new CouponRecord(
                c.Id,
                c.Code,
                c.Name,
                c.Type,
                c.Value,
                c.Currency,
                c.ExpiresAtUtc,
                c.IsActive))
            .ToListAsync(cancellationToken);
}
