using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.Discounts.CalculateOrderDiscounts;

public sealed record DiscountBreakdownResponse(
    decimal OriginalPrice,
    string Currency,
    IReadOnlyList<DiscountLineResponse> Discounts,
    decimal TotalDiscount,
    decimal FinalPrice)
{
    internal static DiscountBreakdownResponse From(DiscountBreakdown breakdown) =>
        new(
            breakdown.OriginalPrice.Amount,
            breakdown.OriginalPrice.Currency.IsoCode,
            breakdown.Lines
                .Select(line => new DiscountLineResponse(
                    line.Name,
                    line.Source,
                    line.Type.ToString(),
                    line.Rule,
                    line.PriceBefore.Amount,
                    line.Deducted.Amount,
                    line.PriceAfter.Amount))
                .ToList(),
            breakdown.TotalDiscount.Amount,
            breakdown.FinalPrice.Amount);
}

public sealed record DiscountLineResponse(
    string Name,
    string Source,
    string Type,
    string Rule,
    decimal PriceBefore,
    decimal Deducted,
    decimal PriceAfter);
