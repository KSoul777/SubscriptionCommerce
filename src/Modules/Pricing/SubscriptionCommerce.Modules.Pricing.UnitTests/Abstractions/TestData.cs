using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.UnitTests.Abstractions;

internal static class TestData
{
    internal static readonly Currency Euro = Currency.Create("EUR").Value;

    internal static Money Eur(decimal amount) => Money.Create(amount, Euro);

    internal static Discount Percentage(string name, decimal percentage, string source = "Test") =>
        Discount.Create(name, source, DiscountType.Percentage, percentage, currency: null).Value;

    internal static Discount FixedAmount(string name, decimal amount, string currency = "EUR", string source = "Test") =>
        Discount.Create(name, source, DiscountType.FixedAmount, amount, currency).Value;
}
