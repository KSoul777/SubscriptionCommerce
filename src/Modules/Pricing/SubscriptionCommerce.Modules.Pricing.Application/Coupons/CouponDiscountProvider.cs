using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.Coupons;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.Coupons;

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

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;

        IReadOnlyList<Coupon> coupons = await reader.GetActiveUnexpiredByCodesAsync(order.CouponCodes, utcNow, cancellationToken);

        var discounts = new List<Discount>();

        foreach (Coupon coupon in coupons.Where(c => c.IsRedeemableAt(utcNow)).OrderBy(c => c.Id))
        {
            Result<Discount> discount = Discount.Create(coupon.Name, Source, coupon.Type, coupon.Value, coupon.Currency);

            if (discount.IsFailure)
            {
                return Result.Failure<IReadOnlyList<Discount>>(
                    PricingErrors.InvalidStoredDiscount(Source, coupon.Id, discount.Error));
            }

            discounts.Add(discount.Value);
        }

        return discounts;
    }
}
