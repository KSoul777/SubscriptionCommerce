using SubscriptionCommerce.Modules.Pricing.Domain.Coupons;

namespace SubscriptionCommerce.Modules.Pricing.Application.Coupons;

public interface ICouponReader
{
    Task<IReadOnlyList<Coupon>> GetActiveUnexpiredByCodesAsync(
        IReadOnlyCollection<string> codes,
        DateTime utcNow,
        CancellationToken cancellationToken);
}
