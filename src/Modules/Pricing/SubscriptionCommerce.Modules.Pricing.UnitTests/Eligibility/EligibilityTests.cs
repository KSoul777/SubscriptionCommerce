using AwesomeAssertions;
using SubscriptionCommerce.Modules.Pricing.Domain.Coupons;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Domain.Promotions;
using static SubscriptionCommerce.Modules.Pricing.UnitTests.Abstractions.TestData;

namespace SubscriptionCommerce.Modules.Pricing.UnitTests.Eligibility;

public class EligibilityTests
{
    private static readonly DateTime UtcNow = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(1, true, true)]
    [InlineData(1, false, false)]
    [InlineData(2, true, false)]
    public void PriceListDiscount_Should_BeEligible_OnlyWhenActiveAndForItsCustomer(int customerId, bool isActive, bool expected)
    {
        var discount = PriceListDiscount.Create(1, "Gold", DiscountType.Percentage, 5m, null, isActive);

        discount.IsEligibleFor(customerId).Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, 1, true, true)]
    [InlineData(0, 1, true, true)]
    [InlineData(-1, 0, true, false)]
    [InlineData(1, 2, true, false)]
    [InlineData(-2, -1, true, false)]
    [InlineData(-1, 1, false, false)]
    public void Promotion_Should_BeEligible_OnlyWhileActiveInsideItsWindow(int startDays, int endDays, bool isActive, bool expected)
    {
        var promotion = Promotion.Create(
            "Autumn", DiscountType.Percentage, 10m, null, UtcNow.AddDays(startDays), UtcNow.AddDays(endDays), isActive);

        promotion.IsEligibleAt(UtcNow).Should().Be(expected);
    }

    [Fact]
    public void Coupon_Should_NeverExpire_WithoutAnExpiryDate()
    {
        var coupon = Coupon.Create("WELCOME10", "Welcome", DiscountType.FixedAmount, 10m, Euro, expiresAtUtc: null);

        coupon.IsRedeemableAt(UtcNow.AddYears(100)).Should().BeTrue();
    }

    [Theory]
    [InlineData(1, true, true)]
    [InlineData(0, true, false)]
    [InlineData(-1, true, false)]
    [InlineData(1, false, false)]
    public void Coupon_Should_BeRedeemable_OnlyWhileActiveAndNotExpired(int expiresInDays, bool isActive, bool expected)
    {
        var coupon = Coupon.Create("SPRING", "Spring", DiscountType.FixedAmount, 5m, Euro, UtcNow.AddDays(expiresInDays), isActive);

        coupon.IsRedeemableAt(UtcNow).Should().Be(expected);
    }

    [Fact]
    public void Coupon_Should_StoreItsCodeUpperCase()
    {
        var coupon = Coupon.Create(" welcome10 ", "Welcome", DiscountType.FixedAmount, 10m, Euro, expiresAtUtc: null);

        coupon.Code.Should().Be("WELCOME10");
    }
}
