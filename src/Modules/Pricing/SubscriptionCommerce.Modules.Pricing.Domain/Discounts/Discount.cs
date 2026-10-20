using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

public abstract class Discount
{
    public const int NameMaxLength = 200;

    private protected Discount(string name, string source)
    {
        Name = name;
        Source = source;
    }

    public string Name { get; }

    public string Source { get; }

    public abstract DiscountType Type { get; }

    public abstract string Rule { get; }

    public static Result<Discount> Create(string? name, string? source, DiscountType type, decimal value, Currency? currency)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > NameMaxLength)
        {
            return Result.Failure<Discount>(DiscountErrors.NameRequired);
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            return Result.Failure<Discount>(DiscountErrors.SourceRequired);
        }

        if (decimal.Round(value, Currency.DecimalPlaces) != value)
        {
            return Result.Failure<Discount>(DiscountErrors.TooManyDecimals);
        }

        return type switch
        {
            DiscountType.Percentage => PercentageDiscount.Create(name.Trim(), source, value, currency),
            DiscountType.FixedAmount => CreateFixedAmount(name.Trim(), source, value, currency),
            _ => Result.Failure<Discount>(DiscountErrors.UnknownType)
        };
    }

    public abstract bool AppliesTo(Currency currency);

    private static Result<Discount> CreateFixedAmount(string name, string source, decimal value, Currency? currency)
    {
        Result<Money> money = Money.Create(value, currency);

        if (money.IsFailure)
        {
            return Result.Failure<Discount>(money.Error);
        }

        return FixedAmountDiscount.Create(name, source, money.Value);
    }

    internal abstract Result<Money> CalculateDeduction(Money runningPrice);
}
