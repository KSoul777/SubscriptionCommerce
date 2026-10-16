using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

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
        Result<Discount> result = Discount.Create("Promo", "Test", DiscountType.Percentage, 10m, "EUR");

        result.Error.Should().Be(DiscountErrors.PercentageWithCurrency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_Should_Fail_WhenFixedAmountIsNotPositive(decimal amount)
    {
        Result<Discount> result = Discount.Create("Coupon", "Test", DiscountType.FixedAmount, amount, "EUR");

        result.Error.Should().Be(DiscountErrors.AmountNotPositive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("E1R")]
    public void Create_Should_Fail_WhenFixedAmountHasNoValidCurrency(string? currency)
    {
        Result<Discount> result = Discount.Create("Coupon", "Test", DiscountType.FixedAmount, 10m, currency);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Money.");
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
    public void Create_Should_NormalizeTheCurrency()
    {
        Result<Discount> result = Discount.Create("Coupon", "Test", DiscountType.FixedAmount, 10m, " eur ");

        result.Value.Currency!.IsoCode.Should().Be("EUR");
        result.Value.Rule.Should().Be("10.00 EUR");
    }
}
