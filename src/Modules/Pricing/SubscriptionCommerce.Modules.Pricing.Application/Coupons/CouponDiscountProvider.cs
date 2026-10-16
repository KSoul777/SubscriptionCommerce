using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.Coupons;

// Eligible: the customer entered the code, the coupon is active and it has not expired.
// Unknown or invalid codes are skipped, not reported as errors.
internal sealed class CouponDiscountProvider(ICouponReader reader, TimeProvider timeProvider) : IDiscountProvider
{
    internal const string Source = "Coupon";

    public int Priority => 300;

    public async Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        if (order.CouponCodes.Count == 0)
        {
            return Array.Empty<Discount>();
        }

        IReadOnlyList<CouponRecord> records = await reader.GetByCodesAsync(order.CouponCodes, cancellationToken);

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;

        IEnumerable<CouponRecord> eligible = records
            .Where(r => r.IsActive && (r.ExpiresAtUtc is null || utcNow < r.ExpiresAtUtc))
            .OrderBy(r => r.Id);

        var discounts = new List<Discount>();

        foreach (CouponRecord record in eligible)
        {
            Result<Discount> discount = Discount.Create(record.Name, Source, record.Type, record.Value, record.Currency);

            if (discount.IsFailure)
            {
                return Result.Failure<IReadOnlyList<Discount>>(
                    PricingErrors.InvalidStoredDiscount(Source, record.Id, discount.Error));
            }

            discounts.Add(discount.Value);
        }

        return discounts;
    }
}
