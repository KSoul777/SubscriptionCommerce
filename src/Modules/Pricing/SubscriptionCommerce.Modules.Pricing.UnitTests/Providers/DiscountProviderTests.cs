using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Application.Coupons;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Application.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Application.Promotions;
using SubscriptionCommerce.Modules.Pricing.Domain.Coupons;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Domain.Promotions;
using SubscriptionCommerce.Modules.Pricing.UnitTests.Abstractions;
using static SubscriptionCommerce.Modules.Pricing.UnitTests.Abstractions.TestData;

namespace SubscriptionCommerce.Modules.Pricing.UnitTests.Providers;

public class DiscountProviderTests
{
    private static readonly DateTime UtcNow = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);

    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    private readonly ListLogger<PromotionDiscountProvider> _promotionLogger = new();

    public DiscountProviderTests()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(UtcNow));
    }

    [Fact]
    public async Task PriceList_Should_ReturnOnlyEligibleDiscounts()
    {
        IPriceListReader reader = Substitute.For<IPriceListReader>();
        reader.GetActiveForCustomerAsync(1, Arg.Any<CancellationToken>()).Returns(
        [
            PriceListDiscount.Create(1, "Silver", DiscountType.Percentage, 3m, null, isActive: false),
            PriceListDiscount.Create(1, "Gold", DiscountType.Percentage, 5m, null)
        ]);

        Result<IReadOnlyList<Discount>> result = await new PriceListDiscountProvider(reader)
            .GetEligibleDiscountsAsync(OrderWith(), CancellationToken.None);

        result.Value.Select(d => d.Name).Should().Equal("Gold");
        result.Value.Single().Source.Should().Be("PriceList");
    }

    [Fact]
    public async Task PriceList_Should_Fail_WhenAStoredDiscountIsInvalid()
    {
        IPriceListReader reader = Substitute.For<IPriceListReader>();
        reader.GetActiveForCustomerAsync(1, Arg.Any<CancellationToken>()).Returns(
            [PriceListDiscount.Create(1, "Broken", DiscountType.Percentage, 150m, null)]);

        Result<IReadOnlyList<Discount>> result = await new PriceListDiscountProvider(reader)
            .GetEligibleDiscountsAsync(OrderWith(), CancellationToken.None);

        result.Error.Code.Should().Be("Pricing.InvalidStoredDiscount");
        result.Error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public async Task PriceList_Should_ReturnDiscountsInCreationOrder_WhateverOrderTheReaderReturns()
    {
        IPriceListReader reader = Substitute.For<IPriceListReader>();
        reader.GetActiveForCustomerAsync(1, Arg.Any<CancellationToken>()).Returns(
        [
            PriceListDiscount.Create(1, "Loyalty credit", DiscountType.FixedAmount, 10m, Euro).Set(d => d.Id, 2),
            PriceListDiscount.Create(1, "Gold", DiscountType.Percentage, 5m, null).Set(d => d.Id, 1)
        ]);

        Result<IReadOnlyList<Discount>> result = await new PriceListDiscountProvider(reader)
            .GetEligibleDiscountsAsync(OrderWith(), CancellationToken.None);

        result.Value.Select(d => d.Name).Should().Equal("Gold", "Loyalty credit");
    }

    [Fact]
    public async Task Promotion_Should_SkipAndLog_AnActivePromotionThatEndsBeforeItStarts()
    {
        IPromotionReader reader = Substitute.For<IPromotionReader>();
        reader.GetActiveNotEndedAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(
        [
            Promotion.Create("Current", DiscountType.Percentage, 10m, null, UtcNow.AddDays(-1), UtcNow.AddDays(1)),
            Promotion.Create("Backwards", DiscountType.Percentage, 10m, null, UtcNow.AddDays(1), UtcNow.AddDays(-1))
        ]);

        Result<IReadOnlyList<Discount>> result = await new PromotionDiscountProvider(reader, _timeProvider, _promotionLogger)
            .GetEligibleDiscountsAsync(OrderWith(), CancellationToken.None);

        result.Value.Select(d => d.Name).Should().Equal("Current");
        _promotionLogger.Entries.Should().ContainSingle()
            .Which.Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public async Task Promotion_Should_IgnoreABadWindow_OnAnInactivePromotion()
    {
        IPromotionReader reader = Substitute.For<IPromotionReader>();
        reader.GetActiveNotEndedAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(
        [
            Promotion.Create("Current", DiscountType.Percentage, 10m, null, UtcNow.AddDays(-1), UtcNow.AddDays(1)),
            Promotion.Create("Retired", DiscountType.Percentage, 10m, null, UtcNow.AddDays(1), UtcNow.AddDays(-1), isActive: false)
        ]);

        Result<IReadOnlyList<Discount>> result = await new PromotionDiscountProvider(reader, _timeProvider, _promotionLogger)
            .GetEligibleDiscountsAsync(OrderWith(), CancellationToken.None);

        result.Value.Select(d => d.Name).Should().Equal("Current");
    }

    [Fact]
    public async Task Promotion_Should_ReturnOnlyPromotionsRunningNow()
    {
        IPromotionReader reader = Substitute.For<IPromotionReader>();
        reader.GetActiveNotEndedAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(
        [
            Promotion.Create("Current", DiscountType.Percentage, 10m, null, UtcNow.AddDays(-1), UtcNow.AddDays(1)),
            Promotion.Create("Ended", DiscountType.Percentage, 10m, null, UtcNow.AddDays(-2), UtcNow.AddDays(-1)),
            Promotion.Create("Paused", DiscountType.Percentage, 10m, null, UtcNow.AddDays(-1), UtcNow.AddDays(1), isActive: false)
        ]);

        Result<IReadOnlyList<Discount>> result = await new PromotionDiscountProvider(reader, _timeProvider, _promotionLogger)
            .GetEligibleDiscountsAsync(OrderWith(), CancellationToken.None);

        result.Value.Select(d => d.Name).Should().Equal("Current");
    }

    [Fact]
    public async Task Coupon_Should_NotQueryTheReader_WhenTheOrderHasNoCodes()
    {
        ICouponReader reader = Substitute.For<ICouponReader>();

        Result<IReadOnlyList<Discount>> result = await new CouponDiscountProvider(reader, _timeProvider)
            .GetEligibleDiscountsAsync(OrderWith(), CancellationToken.None);

        result.Value.Should().BeEmpty();
        await reader.DidNotReceiveWithAnyArgs().GetActiveUnexpiredByCodesAsync(default!, default, default);
    }

    [Fact]
    public async Task Coupon_Should_SkipInactiveAndExpiredCoupons()
    {
        ICouponReader reader = Substitute.For<ICouponReader>();
        reader.GetActiveUnexpiredByCodesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(
        [
            Coupon.Create("WELCOME10", "WELCOME10 coupon", DiscountType.FixedAmount, 10m, Euro, expiresAtUtc: null),
            Coupon.Create("SPRING", "SPRING coupon", DiscountType.FixedAmount, 10m, Euro, UtcNow),
            Coupon.Create("OLD", "OLD coupon", DiscountType.FixedAmount, 10m, Euro, expiresAtUtc: null, isActive: false),
            Coupon.Create("LATER", "LATER coupon", DiscountType.FixedAmount, 10m, Euro, UtcNow.AddDays(1))
        ]);

        Result<IReadOnlyList<Discount>> result = await new CouponDiscountProvider(reader, _timeProvider)
            .GetEligibleDiscountsAsync(OrderWith("WELCOME10", "SPRING", "OLD", "LATER"), CancellationToken.None);

        result.Value.Select(d => d.Name).Should().Equal("WELCOME10 coupon", "LATER coupon");
        await reader.Received(1).GetActiveUnexpiredByCodesAsync(
            Arg.Is<IReadOnlyCollection<string>>(codes => codes.Count == 4),
            UtcNow,
            Arg.Any<CancellationToken>());
    }

    private static Order OrderWith(params string[] couponCodes) => new(1, Eur(100m), couponCodes);
}
