using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.Discounts.CalculateOrderDiscounts;

internal sealed class CalculateOrderDiscountsQueryHandler(IEnumerable<IDiscountProvider> providers)
    : IQueryHandler<CalculateOrderDiscountsQuery, DiscountBreakdownResponse>
{
    public async Task<Result<DiscountBreakdownResponse>> Handle(
        CalculateOrderDiscountsQuery query,
        CancellationToken cancellationToken)
    {
        Result<Money> price = Money.Create(query.Price, query.Currency);

        if (price.IsFailure)
        {
            return Result.Failure<DiscountBreakdownResponse>(price.Error);
        }

        string[] couponCodes = query.CouponCodes
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var order = new Order(query.CustomerId, price.Value, couponCodes);

        var sources = new List<PrioritizedDiscounts>();

        foreach (IDiscountProvider provider in providers.OrderBy(p => p.Priority))
        {
            Result<IReadOnlyList<Discount>> eligible = await provider.GetEligibleDiscountsAsync(order, cancellationToken);

            if (eligible.IsFailure)
            {
                return Result.Failure<DiscountBreakdownResponse>(eligible.Error);
            }

            sources.Add(new PrioritizedDiscounts(
                provider.Priority,
                eligible.Value.Where(d => d.AppliesTo(price.Value.Currency)).ToList()));
        }

        Result<DiscountBreakdown> breakdown = DiscountBreakdown.Apply(price.Value, sources);

        if (breakdown.IsFailure)
        {
            return Result.Failure<DiscountBreakdownResponse>(breakdown.Error);
        }

        return DiscountBreakdownResponse.From(breakdown.Value);
    }
}
