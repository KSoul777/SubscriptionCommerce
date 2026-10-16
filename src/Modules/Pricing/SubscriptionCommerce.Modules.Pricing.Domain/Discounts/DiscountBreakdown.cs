using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

public sealed record DiscountLine(
    string Name,
    string Source,
    DiscountType Type,
    string Rule,
    Money PriceBefore,
    Money Deducted,
    Money PriceAfter);

// Applies discounts, already in priority order, to a running price.
// Each discount works on the price left by the previous one, not on the original price.
public sealed class DiscountBreakdown
{
    private DiscountBreakdown(Money originalPrice, IReadOnlyList<DiscountLine> lines, Money finalPrice)
    {
        OriginalPrice = originalPrice;
        Lines = lines;
        FinalPrice = finalPrice;
    }

    public Money OriginalPrice { get; }

    public IReadOnlyList<DiscountLine> Lines { get; }

    public Money FinalPrice { get; }

    public Money TotalDiscount => (OriginalPrice - FinalPrice).Value;

    public static Result<DiscountBreakdown> Apply(Money originalPrice, IEnumerable<Discount> discountsInPriorityOrder)
    {
        if (originalPrice.Amount < 0m)
        {
            return Result.Failure<DiscountBreakdown>(DiscountErrors.NegativePrice);
        }

        var lines = new List<DiscountLine>();
        Money runningPrice = originalPrice;

        foreach (Discount discount in discountsInPriorityOrder)
        {
            Result<Money> deduction = discount.CalculateDeduction(runningPrice);

            if (deduction.IsFailure)
            {
                return Result.Failure<DiscountBreakdown>(deduction.Error);
            }

            Money priceAfter = (runningPrice - deduction.Value).Value;

            lines.Add(new DiscountLine(
                discount.Name,
                discount.Source,
                discount.Type,
                discount.Rule,
                runningPrice,
                deduction.Value,
                priceAfter));

            runningPrice = priceAfter;
        }

        return new DiscountBreakdown(originalPrice, lines, runningPrice);
    }
}
