using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using static SubscriptionCommerce.Modules.Pricing.UnitTests.Abstractions.TestData;

namespace SubscriptionCommerce.Modules.Pricing.UnitTests.Discounts;

public class DiscountBreakdownTests
{
    [Fact]
    public void Apply_Should_MatchTheBriefExample()
    {
        // Arrange: 340 EUR, price list 5%, promotion 10%, coupon -10 EUR.
        Discount[] discounts =
        [
            Percentage("Price list", 5m),
            Percentage("Promotion", 10m),
            FixedAmount("Coupon", 10m)
        ];

        // Act
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(Eur(340m), discounts);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Lines.Select(l => l.Deducted.Amount).Should().Equal(17.00m, 32.30m, 10.00m);
        result.Value.Lines.Select(l => l.PriceAfter.Amount).Should().Equal(323.00m, 290.70m, 280.70m);
        result.Value.TotalDiscount.Should().Be(Eur(59.30m));
        result.Value.FinalPrice.Should().Be(Eur(280.70m));
    }

    [Fact]
    public void Apply_Should_ApplyEachDiscountToTheRunningPrice()
    {
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(
            Eur(100m),
            [Percentage("First", 50m), Percentage("Second", 50m)]);

        // 50% of 100, then 50% of the remaining 50; not 50% of 100 twice.
        result.Value.Lines[1].PriceBefore.Should().Be(Eur(50m));
        result.Value.FinalPrice.Should().Be(Eur(25m));
    }

    [Fact]
    public void Apply_Should_CapFixedAmount_WhenItIsLargerThanTheRemainingPrice()
    {
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(
            Eur(15m),
            [FixedAmount("Large coupon", 500m)]);

        result.IsSuccess.Should().BeTrue();
        result.Value.Lines.Single().Deducted.Should().Be(Eur(15m));
        result.Value.FinalPrice.Should().Be(Eur(0m));
    }

    [Fact]
    public void Apply_Should_RoundEachLineToCentsAwayFromZero()
    {
        // 10% of 0.25 is 0.025: away-from-zero gives 0.03 (banker's rounding would give 0.02).
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(Eur(0.25m), [Percentage("Ten percent", 10m)]);

        result.Value.Lines.Single().Deducted.Should().Be(Eur(0.03m));
        result.Value.FinalPrice.Should().Be(Eur(0.22m));
    }

    [Fact]
    public void Apply_Should_Fail_WhenFixedAmountCurrencyDiffersFromThePrice()
    {
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(
            Eur(100m),
            [FixedAmount("Dollar coupon", 10m, "USD")]);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MoneyErrors.CurrencyMismatch);
    }

    [Fact]
    public void Apply_Should_ReturnTheOriginalPrice_WhenThereAreNoDiscounts()
    {
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(Eur(340m), []);

        result.Value.Lines.Should().BeEmpty();
        result.Value.TotalDiscount.Should().Be(Eur(0m));
        result.Value.FinalPrice.Should().Be(Eur(340m));
    }

    [Fact]
    public void Apply_Should_Fail_WhenThePriceIsNegative()
    {
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(Eur(-1m), []);

        result.Error.Should().Be(DiscountErrors.NegativePrice);
    }
}
