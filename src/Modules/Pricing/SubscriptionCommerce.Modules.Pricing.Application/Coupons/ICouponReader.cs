using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.Coupons;

public sealed record CouponRecord(
    int Id,
    string Code,
    string Name,
    DiscountType Type,
    decimal Value,
    string? Currency,
    DateTime? ExpiresAtUtc,
    bool IsActive);

public interface ICouponReader
{
    Task<IReadOnlyList<CouponRecord>> GetByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken);
}
