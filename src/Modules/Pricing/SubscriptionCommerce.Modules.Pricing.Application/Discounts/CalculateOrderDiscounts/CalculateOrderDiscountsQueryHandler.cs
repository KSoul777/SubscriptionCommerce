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

        var discounts = new List<Discount>();

        // Sequential on purpose: providers share the request's DbContext, which is not thread-safe.
        // OrderBy is stable, so each provider keeps its own order for the discounts it returns.
        foreach (IDiscountProvider provider in providers.OrderBy(p => p.Priority))
        {
            Result<IReadOnlyList<Discount>> eligible = await provider.GetEligibleDiscountsAsync(order, cancellationToken);

            if (eligible.IsFailure)
            {
                return Result.Failure<DiscountBreakdownResponse>(eligible.Error);
            }

            discounts.AddRange(eligible.Value);
        }

        Result<DiscountBreakdown> breakdown = DiscountBreakdown.Apply(price.Value, discounts);

        if (breakdown.IsFailure)
        {
            return Result.Failure<DiscountBreakdownResponse>(breakdown.Error);
        }

        return DiscountBreakdownResponse.From(breakdown.Value);
    }
}
