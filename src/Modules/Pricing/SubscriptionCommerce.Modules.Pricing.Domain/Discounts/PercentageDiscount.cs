using System.Globalization;
using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

public sealed class PercentageDiscount : Discount
{
    private PercentageDiscount(string name, string source, decimal percentage)
        : base(name, source)
    {
        Percentage = percentage;
    }

    public decimal Percentage { get; }

    public override DiscountType Type => DiscountType.Percentage;

    public override string Rule => $"{Percentage.ToString("0.##", CultureInfo.InvariantCulture)}%";

    public override bool AppliesTo(Currency currency) => true;

    internal static Result<Discount> Create(string name, string source, decimal percentage, Currency? currency)
    {
        if (percentage is <= 0m or > 100m)
        {
            return Result.Failure<Discount>(DiscountErrors.PercentageOutOfRange);
        }

        if (currency is not null)
        {
            return Result.Failure<Discount>(DiscountErrors.PercentageWithCurrency);
        }

        return new PercentageDiscount(name, source, percentage);
    }

    internal override Result<Money> CalculateDeduction(Money runningPrice) =>
        (runningPrice * (Percentage / 100m)).Round();
}
