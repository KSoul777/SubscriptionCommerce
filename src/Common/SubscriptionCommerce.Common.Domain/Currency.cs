namespace SubscriptionCommerce.Common.Domain;

public sealed record Currency
{
    private Currency(string isoCode) => IsoCode = isoCode;

    public string IsoCode { get; }

    public static Result<Currency> Create(string? isoCode)
    {
        if (string.IsNullOrWhiteSpace(isoCode))
        {
            return Result.Failure<Currency>(MoneyErrors.CurrencyRequired);
        }

        string normalized = isoCode.Trim().ToUpperInvariant();

        if (normalized.Length != 3 || !normalized.All(char.IsAsciiLetterUpper))
        {
            return Result.Failure<Currency>(MoneyErrors.CurrencyInvalid);
        }

        return new Currency(normalized);
    }

    public override string ToString() => IsoCode;
}
