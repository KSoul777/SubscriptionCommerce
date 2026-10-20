using System.Globalization;

namespace SubscriptionCommerce.Common.Domain;

public sealed record Money
{
    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public Currency Currency { get; }

    public static Result<Money> Create(decimal amount, Currency? currency)
    {
        if (currency is null)
        {
            return Result.Failure<Money>(MoneyErrors.CurrencyRequired);
        }

        return Result.Success(new Money(amount, currency));
    }

    public static Result<Money> Create(decimal amount, string? currency)
    {
        Result<Currency> result = Currency.Create(currency);

        return result.IsFailure
            ? Result.Failure<Money>(result.Error)
            : Create(amount, result.Value);
    }

    public static Money Zero(Currency currency) => new(0m, currency);

    public static Result<Money> operator -(Money left, Money right) => left.Subtract(right);

    public static Money operator *(Money left, decimal right) => new(left.Amount * right, left.Currency);

    public Money Round() =>
        new(Math.Round(Amount, Currency.DecimalPlaces, MidpointRounding.AwayFromZero), Currency);

    private Result<Money> Subtract(Money other)
    {
        if (other.Currency != Currency)
        {
            return Result.Failure<Money>(MoneyErrors.CurrencyMismatch);
        }

        return new Money(Amount - other.Amount, Currency);
    }

    public override string ToString() =>
        $"{Amount.ToString("0.00", CultureInfo.InvariantCulture)} {Currency}";
}
