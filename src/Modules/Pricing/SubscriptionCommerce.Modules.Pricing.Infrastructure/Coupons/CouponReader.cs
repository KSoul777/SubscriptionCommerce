using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Pricing.Application.Coupons;
using SubscriptionCommerce.Modules.Pricing.Domain.Coupons;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Coupons;

internal sealed class CouponReader(PricingDbContext dbContext) : ICouponReader
{
    public async Task<IReadOnlyList<Coupon>> GetActiveUnexpiredByCodesAsync(
        IReadOnlyCollection<string> codes,
        DateTime utcNow,
        CancellationToken cancellationToken) =>
        await dbContext.Coupons
            .AsNoTracking()
            .Where(c => codes.Contains(c.Code) && c.IsActive && (c.ExpiresAtUtc == null || c.ExpiresAtUtc > utcNow))
            .ToListAsync(cancellationToken);
}
