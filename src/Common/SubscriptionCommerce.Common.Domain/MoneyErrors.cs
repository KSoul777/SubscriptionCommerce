namespace SubscriptionCommerce.Common.Domain;

public static class MoneyErrors
{
    public static readonly Error CurrencyRequired = Error.Problem(
        "Money.CurrencyRequired",
        "A currency is required");

    public static readonly Error CurrencyInvalid = Error.Problem(
        "Money.CurrencyInvalid",
        "The currency must be a three-letter ISO 4217 code");

    public static readonly Error CurrencyMismatch = Error.Problem(
        "Money.CurrencyMismatch",
        "Amounts in different currencies can't be combined");
}
