using System.Reflection;
using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using static SubscriptionCommerce.Modules.Pricing.UnitTests.Abstractions.TestData;

namespace SubscriptionCommerce.Modules.Pricing.UnitTests.Discounts;

public class DiscountTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(100.01)]
    public void Create_Should_Fail_WhenPercentageIsOutOfRange(decimal percentage)
    {
        Result<Discount> result = Discount.Create("Promo", "Test", DiscountType.Percentage, percentage, currency: null);

        result.Error.Should().Be(DiscountErrors.PercentageOutOfRange);
    }

    [Fact]
    public void Create_Should_AcceptOneHundredPercent()
    {
        Result<Discount> result = Discount.Create("Free", "Test", DiscountType.Percentage, 100m, currency: null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Rule.Should().Be("100%");
    }

    [Fact]
    public void Create_Should_Fail_WhenPercentageHasACurrency()
    {
        Result<Discount> result = Discount.Create("Promo", "Test", DiscountType.Percentage, 10m, Euro);

        result.Error.Should().Be(DiscountErrors.PercentageWithCurrency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_Should_Fail_WhenFixedAmountIsNotPositive(decimal amount)
    {
        Result<Discount> result = Discount.Create("Coupon", "Test", DiscountType.FixedAmount, amount, Euro);

        result.Error.Should().Be(DiscountErrors.AmountNotPositive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_Should_ReportTheMissingCurrencyFirst_WhenAFixedAmountIsNotPositiveAndHasNoCurrency(decimal amount)
    {
        Result<Discount> result = Discount.Create("Coupon", "Test", DiscountType.FixedAmount, amount, currency: null);

        result.Error.Should().Be(MoneyErrors.CurrencyRequired);
    }

    [Fact]
    public void Create_Should_Fail_WhenAFixedAmountHasNoCurrency()
    {
        Result<Discount> result = Discount.Create("Coupon", "Test", DiscountType.FixedAmount, 10m, currency: null);

        result.Error.Should().Be(MoneyErrors.CurrencyRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void Create_Should_Fail_WhenNameIsMissing(string? name)
    {
        Result<Discount> result = Discount.Create(name, "Test", DiscountType.Percentage, 10m, currency: null);

        result.Error.Should().Be(DiscountErrors.NameRequired);
    }

    [Fact]
    public void Create_Should_Fail_WhenTypeIsUnknown()
    {
        Result<Discount> result = Discount.Create("Promo", "Test", (DiscountType)99, 10m, currency: null);

        result.Error.Should().Be(DiscountErrors.UnknownType);
    }

    [Fact]
    public void Create_Should_ShowTheAmountAndCurrencyInTheRule()
    {
        Result<Discount> result = Discount.Create("Coupon", "Test", DiscountType.FixedAmount, 10m, Euro);

        result.Value.Rule.Should().Be("10.00 EUR");
    }

    [Theory]
    [InlineData(DiscountType.Percentage, 12.345, null)]
    [InlineData(DiscountType.FixedAmount, 9.999, "EUR")]
    public void Create_Should_Fail_WhenTheValueHasMoreThanTwoDecimals(DiscountType type, decimal value, string? currency)
    {
        Result<Discount> result = Discount.Create("Promo", "Test", type, value, CurrencyOf(currency));

        result.Error.Should().Be(DiscountErrors.TooManyDecimals);
    }

    [Fact]
    public void Create_Should_ShowTheFullPrecisionInTheRule()
    {
        Result<Discount> result = Discount.Create("Promo", "Test", DiscountType.Percentage, 12.35m, currency: null);

        result.Value.Rule.Should().Be("12.35%");
    }

    [Theory]
    [InlineData(DiscountType.Percentage, null, "EUR", true)]
    [InlineData(DiscountType.Percentage, null, "USD", true)]
    [InlineData(DiscountType.FixedAmount, "EUR", "EUR", true)]
    [InlineData(DiscountType.FixedAmount, "USD", "EUR", false)]
    public void AppliesTo_Should_MatchFixedAmountsOnlyToTheirOwnCurrency(
        DiscountType type,
        string? discountCurrency,
        string orderCurrency,
        bool expected)
    {
        Discount discount = Discount.Create("Promo", "Test", type, 10m, CurrencyOf(discountCurrency)).Value;

        discount.AppliesTo(Currency.Create(orderCurrency).Value).Should().Be(expected);
    }

    [Fact]
    public void Create_Should_BuildAPercentageDiscount_ForAPercentage()
    {
        Result<Discount> result = Discount.Create("Promo", "Test", DiscountType.Percentage, 10m, currency: null);

        result.Value.Should().BeOfType<PercentageDiscount>().Which.Percentage.Should().Be(10m);
        result.Value.Type.Should().Be(DiscountType.Percentage);
    }

    [Fact]
    public void Create_Should_HoldAFixedAmountAsMoney()
    {
        Result<Discount> result = Discount.Create("Coupon", "Test", DiscountType.FixedAmount, 10m, Euro);

        result.Value.Should().BeOfType<FixedAmountDiscount>().Which.Amount.Should().Be(Eur(10m));
        result.Value.Type.Should().Be(DiscountType.FixedAmount);
    }

    [Fact]
    public void Discount_Should_OnlyBeSubclassedInsideTheDomain()
    {
        typeof(Discount).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Should().OnlyContain(c => c.IsFamilyAndAssembly);
        typeof(PercentageDiscount).IsSealed.Should().BeTrue();
        typeof(FixedAmountDiscount).IsSealed.Should().BeTrue();
    }

    private static Currency? CurrencyOf(string? isoCode) => isoCode is null ? null : Currency.Create(isoCode).Value;
}
