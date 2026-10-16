using AwesomeAssertions;
using NSubstitute;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Application.Coupons;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Application.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Application.Promotions;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using static SubscriptionCommerce.Modules.Pricing.UnitTests.Abstractions.TestData;

namespace SubscriptionCommerce.Modules.Pricing.UnitTests.Providers;

public class DiscountProviderTests
{
    private static readonly DateTime UtcNow = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);

    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    public DiscountProviderTests()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(UtcNow));
    }

    [Fact]
    public async Task PriceList_Should_SkipInactiveDiscounts()
    {
        IPriceListReader reader = Substitute.For<IPriceListReader>();
        reader.GetForCustomerAsync(1, Arg.Any<CancellationToken>()).Returns(
        [
            new PriceListDiscountRecord(2, "Silver", DiscountType.Percentage, 3m, null, IsActive: false),
            new PriceListDiscountRecord(1, "Gold", DiscountType.Percentage, 5m, null, IsActive: true)
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
        reader.GetForCustomerAsync(1, Arg.Any<CancellationToken>()).Returns(
            [new PriceListDiscountRecord(7, "Broken", DiscountType.Percentage, 150m, null, IsActive: true)]);

        Result<IReadOnlyList<Discount>> result = await new PriceListDiscountProvider(reader)
            .GetEligibleDiscountsAsync(OrderWith(), CancellationToken.None);

        result.Error.Code.Should().Be("Pricing.InvalidStoredDiscount");
        result.Error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public async Task Promotion_Should_ReturnOnlyActivePromotionsInsideTheirWindow()
    {
        IPromotionReader reader = Substitute.For<IPromotionReader>();
        reader.GetPromotionsAsync(Arg.Any<CancellationToken>()).Returns(
        [
            Promotion(1, "Current", UtcNow.AddDays(-1), UtcNow.AddDays(1)),
            Promotion(2, "Starts now", UtcNow, UtcNow.AddDays(1)),
            Promotion(3, "Ended now", UtcNow.AddDays(-1), UtcNow),
            Promotion(4, "Future", UtcNow.AddDays(1), UtcNow.AddDays(2)),
            Promotion(5, "Inactive", UtcNow.AddDays(-1), UtcNow.AddDays(1), isActive: false)
        ]);

        Result<IReadOnlyList<Discount>> result = await new PromotionDiscountProvider(reader, _timeProvider)
            .GetEligibleDiscountsAsync(OrderWith(), CancellationToken.None);

        result.Value.Select(d => d.Name).Should().Equal("Current", "Starts now");
    }

    [Fact]
    public async Task Coupon_Should_NotQueryTheReader_WhenTheOrderHasNoCodes()
    {
        ICouponReader reader = Substitute.For<ICouponReader>();

        Result<IReadOnlyList<Discount>> result = await new CouponDiscountProvider(reader, _timeProvider)
            .GetEligibleDiscountsAsync(OrderWith(), CancellationToken.None);

        result.Value.Should().BeEmpty();
        await reader.DidNotReceiveWithAnyArgs().GetByCodesAsync(default!, default);
    }

    [Fact]
    public async Task Coupon_Should_SkipInactiveAndExpiredCoupons()
    {
        ICouponReader reader = Substitute.For<ICouponReader>();
        reader.GetByCodesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>()).Returns(
        [
            Coupon(1, "WELCOME10", expiresAtUtc: null),
            Coupon(2, "SPRING", expiresAtUtc: UtcNow),
            Coupon(3, "OLD", expiresAtUtc: null, isActive: false),
            Coupon(4, "LATER", expiresAtUtc: UtcNow.AddDays(1))
        ]);

        Result<IReadOnlyList<Discount>> result = await new CouponDiscountProvider(reader, _timeProvider)
            .GetEligibleDiscountsAsync(OrderWith("WELCOME10", "SPRING", "OLD", "LATER"), CancellationToken.None);

        result.Value.Select(d => d.Name).Should().Equal("WELCOME10 coupon", "LATER coupon");
        await reader.Received(1).GetByCodesAsync(
            Arg.Is<IReadOnlyCollection<string>>(codes => codes.Count == 4),
            Arg.Any<CancellationToken>());
    }

    private static Order OrderWith(params string[] couponCodes) => new(1, Eur(100m), couponCodes);

    private static PromotionRecord Promotion(int id, string name, DateTime startsAtUtc, DateTime endsAtUtc, bool isActive = true) =>
        new(id, name, DiscountType.Percentage, 10m, null, startsAtUtc, endsAtUtc, isActive);

    private static CouponRecord Coupon(int id, string code, DateTime? expiresAtUtc, bool isActive = true) =>
        new(id, code, $"{code} coupon", DiscountType.FixedAmount, 10m, "EUR", expiresAtUtc, isActive);
}
