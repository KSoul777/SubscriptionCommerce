using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.Promotions;

// Eligible: the promotion is active and now is inside [StartsAtUtc, EndsAtUtc).
internal sealed class PromotionDiscountProvider(IPromotionReader reader, TimeProvider timeProvider) : IDiscountProvider
{
    internal const string Source = "Promotion";

    public int Priority => 200;

    public async Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<PromotionRecord> records = await reader.GetPromotionsAsync(cancellationToken);

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;

        IEnumerable<PromotionRecord> eligible = records
            .Where(r => r.IsActive && r.StartsAtUtc <= utcNow && utcNow < r.EndsAtUtc)
            .OrderBy(r => r.Id);

        var discounts = new List<Discount>();

        foreach (PromotionRecord record in eligible)
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
