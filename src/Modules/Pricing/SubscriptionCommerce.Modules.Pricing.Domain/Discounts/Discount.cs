using System.Globalization;
using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

// A discount a customer is entitled to. Values are validated here, at creation,
// so the calculation never meets an invalid percentage or a fixed amount without a currency.
public sealed class Discount
{
    public const int NameMaxLength = 200;

    private Discount(string name, string source, DiscountType type, decimal value, Currency? currency)
    {
        Name = name;
        Source = source;
        Type = type;
        Value = value;
        Currency = currency;
    }

    public string Name { get; }

    // Where the discount comes from, for example "PriceList". Priority belongs to the provider, not here.
    public string Source { get; }

    public DiscountType Type { get; }

    // Percentage points for Percentage, an amount in Currency for FixedAmount.
    public decimal Value { get; }

    // Only set for FixedAmount.
    public Currency? Currency { get; }

    public string Rule => Type == DiscountType.Percentage
        ? $"{Value.ToString("0.##", CultureInfo.InvariantCulture)}%"
        : $"{Value.ToString("0.00", CultureInfo.InvariantCulture)} {Currency}";

    public static Result<Discount> Create(string? name, string? source, DiscountType type, decimal value, string? currency)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > NameMaxLength)
        {
            return Result.Failure<Discount>(DiscountErrors.NameRequired);
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            return Result.Failure<Discount>(DiscountErrors.SourceRequired);
        }

        switch (type)
        {
            case DiscountType.Percentage:
                if (value is <= 0m or > 100m)
                {
                    return Result.Failure<Discount>(DiscountErrors.PercentageOutOfRange);
                }

                if (currency is not null)
                {
                    return Result.Failure<Discount>(DiscountErrors.PercentageWithCurrency);
                }

                return new Discount(name.Trim(), source, type, value, currency: null);

            case DiscountType.FixedAmount:
                if (value <= 0m)
                {
                    return Result.Failure<Discount>(DiscountErrors.AmountNotPositive);
                }

                Result<Currency> fixedCurrency = Currency.Create(currency);

                if (fixedCurrency.IsFailure)
                {
                    return Result.Failure<Discount>(fixedCurrency.Error);
                }

                return new Discount(name.Trim(), source, type, value, fixedCurrency.Value);

            default:
                return Result.Failure<Discount>(DiscountErrors.UnknownType);
        }
    }

    // The amount to take off the running price, rounded to cents and never more than the running price.
    internal Result<Money> CalculateDeduction(Money runningPrice)
    {
        if (Type == DiscountType.Percentage)
        {
            return (runningPrice * (Value / 100m)).Round(2);
        }

        if (Currency != runningPrice.Currency)
        {
            return Result.Failure<Money>(MoneyErrors.CurrencyMismatch);
        }

        return Money.Create(Math.Min(Value, runningPrice.Amount), runningPrice.Currency).Round(2);
    }
}
