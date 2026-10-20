using Microsoft.Extensions.Logging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.Promotions;

namespace SubscriptionCommerce.Modules.Pricing.Application.Promotions;

internal sealed class PromotionDiscountProvider(
    IPromotionReader reader,
    TimeProvider timeProvider,
    ILogger<PromotionDiscountProvider> logger)
    : IDiscountProvider
{
    internal const string Source = "Promotion";

    public int Priority => 200;

    public async Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;

        IReadOnlyList<Promotion> promotions = await reader.GetActiveNotEndedAsync(utcNow, cancellationToken);

        var discounts = new List<Discount>();

        foreach (Promotion promotion in promotions.Where(p => p.IsActive).OrderBy(p => p.Id))
        {
            if (!promotion.HasValidWindow)
            {
                logger.LogWarning("Promotion {PromotionId} ends before it starts, so it can never apply and is skipped", promotion.Id);
                continue;
            }

            if (!promotion.IsEligibleAt(utcNow))
            {
                continue;
            }

            Result<Discount> discount = Discount.Create(promotion.Name, Source, promotion.Type, promotion.Value, promotion.Currency);

            if (discount.IsFailure)
            {
                return Result.Failure<IReadOnlyList<Discount>>(
                    PricingErrors.InvalidStoredDiscount(Source, promotion.Id, discount.Error));
            }

            discounts.Add(discount.Value);
        }

        return discounts;
    }
}
