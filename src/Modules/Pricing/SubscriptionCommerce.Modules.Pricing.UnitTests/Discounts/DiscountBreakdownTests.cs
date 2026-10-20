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
        PrioritizedDiscounts[] sources =
        [
            new(100, [Percentage("Price list", 5m)]),
            new(200, [Percentage("Promotion", 10m)]),
            new(300, [FixedAmount("Coupon", 10m)])
        ];

        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(Eur(340m), sources);

        result.IsSuccess.Should().BeTrue();
        result.Value.Lines.Select(l => l.Deducted.Amount).Should().Equal(17.00m, 32.30m, 10.00m);
        result.Value.Lines.Select(l => l.PriceAfter.Amount).Should().Equal(323.00m, 290.70m, 280.70m);
        result.Value.TotalDiscount.Should().Be(Eur(59.30m));
        result.Value.FinalPrice.Should().Be(Eur(280.70m));
    }

    [Fact]
    public void Apply_Should_ApplySourcesInPriorityOrder_WhateverOrderTheyArriveIn()
    {
        PrioritizedDiscounts[] sources =
        [
            new(300, [FixedAmount("Coupon", 10m)]),
            new(100, [Percentage("Price list", 5m)]),
            new(200, [Percentage("Promotion B", 10m), Percentage("Promotion A", 20m)])
        ];

        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(Eur(340m), sources);

        result.Value.Lines.Select(l => l.Name).Should().Equal("Price list", "Promotion B", "Promotion A", "Coupon");
    }

    [Fact]
    public void Apply_Should_KeepTheOrderWithinASource_BecauseItChangesThePrice()
    {
        Result<DiscountBreakdown> percentFirst = DiscountBreakdown.Apply(
            Eur(100m), OneSource(Percentage("Ten percent", 10m), FixedAmount("Ten off", 10m)));
        Result<DiscountBreakdown> fixedFirst = DiscountBreakdown.Apply(
            Eur(100m), OneSource(FixedAmount("Ten off", 10m), Percentage("Ten percent", 10m)));

        percentFirst.Value.FinalPrice.Should().Be(Eur(80.00m));
        fixedFirst.Value.FinalPrice.Should().Be(Eur(81.00m));
    }

    [Fact]
    public void Lines_Should_RefuseChanges_EvenWhenCastBackToAList()
    {
        DiscountBreakdown breakdown = DiscountBreakdown.Apply(Eur(100m), OneSource(Percentage("Ten percent", 10m))).Value;

        var lines = (IList<DiscountLine>)breakdown.Lines;

        breakdown.Lines.Should().NotBeAssignableTo<List<DiscountLine>>();
        lines.Invoking(l => l.Clear()).Should().Throw<NotSupportedException>();
        breakdown.Lines.Should().ContainSingle();
        breakdown.FinalPrice.Should().Be(Eur(90m));
    }

    [Fact]
    public void Apply_Should_ApplyEachDiscountToTheRunningPrice()
    {
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(
            Eur(100m),
            OneSource(Percentage("First", 50m), Percentage("Second", 50m)));

        result.Value.Lines[1].PriceBefore.Should().Be(Eur(50m));
        result.Value.FinalPrice.Should().Be(Eur(25m));
    }

    [Fact]
    public void Apply_Should_CapFixedAmount_WhenItIsLargerThanTheRemainingPrice()
    {
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(
            Eur(15m),
            OneSource(FixedAmount("Large coupon", 500m)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Lines.Single().Deducted.Should().Be(Eur(15m));
        result.Value.FinalPrice.Should().Be(Eur(0m));
    }

    [Fact]
    public void Apply_Should_RoundEachLineToCentsAwayFromZero()
    {
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(Eur(0.25m), OneSource(Percentage("Ten percent", 10m)));

        result.Value.Lines.Single().Deducted.Should().Be(Eur(0.03m));
        result.Value.FinalPrice.Should().Be(Eur(0.22m));
    }

    [Fact]
    public void Apply_Should_Fail_WhenFixedAmountCurrencyDiffersFromThePrice()
    {
        Result<DiscountBreakdown> result = DiscountBreakdown.Apply(
            Eur(100m),
            OneSource(FixedAmount("Dollar coupon", 10m, "USD")));

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
