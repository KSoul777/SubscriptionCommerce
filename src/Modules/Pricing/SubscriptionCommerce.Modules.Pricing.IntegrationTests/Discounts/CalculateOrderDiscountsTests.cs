using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts.CalculateOrderDiscounts;
using SubscriptionCommerce.Modules.Pricing.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Pricing.IntegrationTests.Discounts;

public sealed class CalculateOrderDiscountsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private static readonly DateTime UtcNow = DateTime.UtcNow;

    private static readonly string[] Welcome10 = ["WELCOME10"];

    private static readonly string[] Yen500 = ["YEN500"];

    [Fact]
    public async Task Should_ApplyTheBriefsExample_ToTheSeededCustomer()
    {
        DiscountBreakdownResponse breakdown = await CalculateAsync(customerId: 1, "welcome10");

        breakdown.Discounts.Select(d => (d.Source, d.Rule, d.PriceBefore, d.Deducted, d.PriceAfter)).Should().Equal(
            ("PriceList", "5%", 340m, 17.00m, 323.00m),
            ("Promotion", "10%", 323.00m, 32.30m, 290.70m),
            ("Coupon", "10.00 EUR", 290.70m, 10.00m, 280.70m));
        breakdown.TotalDiscount.Should().Be(59.30m);
        breakdown.FinalPrice.Should().Be(280.70m);
    }

    [Fact]
    public async Task Should_ReadOnlyTheCustomersActivePriceListDiscounts()
    {
        await ExecuteSqlAsync($"""
            INSERT INTO pricing.PriceListDiscounts (CustomerId, Name, Type, Value, Currency, IsActive)
            VALUES (1001, 'Silver price list', 'Percentage', 3, NULL, 1),
                   (1001, 'Old price list', 'Percentage', 50, NULL, 0),
                   (1002, 'Someone else''s price list', 'Percentage', 20, NULL, 1)
            """);

        DiscountBreakdownResponse breakdown = await CalculateAsync(customerId: 1001);

        breakdown.Discounts.Where(d => d.Source == "PriceList").Select(d => d.Name).Should().Equal("Silver price list");
    }

    [Fact]
    public async Task Should_SkipPromotionsThatAreInactiveExpiredOrNotStartedYet()
    {
        await ExecuteSqlAsync($"""
            INSERT INTO pricing.Promotions (Name, Type, Value, Currency, StartsAtUtc, EndsAtUtc, IsActive)
            VALUES ('Paused promotion', 'Percentage', 20, NULL, {UtcNow.AddDays(-1)}, {UtcNow.AddDays(1)}, 0),
                   ('Expired promotion', 'Percentage', 20, NULL, {UtcNow.AddDays(-10)}, {UtcNow.AddDays(-1)}, 1),
                   ('Future promotion', 'FixedAmount', 5, 'EUR', {UtcNow.AddYears(50)}, {UtcNow.AddYears(51)}, 1)
            """);

        DiscountBreakdownResponse breakdown = await CalculateAsync(customerId: 1003);

        breakdown.Discounts.Where(d => d.Source == "Promotion").Select(d => d.Name).Should().Equal("Autumn promotion");
    }

    [Fact]
    public async Task Should_ApplyOnlyActiveUnexpiredCouponsTheCustomerEntered()
    {
        await ExecuteSqlAsync($"""
            INSERT INTO pricing.Coupons (Code, Name, Type, Value, Currency, ExpiresAtUtc, IsActive)
            VALUES ('TAKE2', 'Two off', 'FixedAmount', 2, 'EUR', {UtcNow.AddDays(30)}, 1),
                   ('OFF5', 'Withdrawn coupon', 'FixedAmount', 5, 'EUR', NULL, 0),
                   ('EXPIRED5', 'Expired coupon', 'FixedAmount', 5, 'EUR', {UtcNow.AddDays(-1)}, 1),
                   ('NOTENTERED', 'Coupon nobody entered', 'Percentage', 50, NULL, NULL, 1)
            """);

        DiscountBreakdownResponse breakdown = await CalculateAsync(customerId: 1004, "take2", " off5", "EXPIRED5", "NOSUCHCODE");

        breakdown.Discounts.Where(d => d.Source == "Coupon").Select(d => (d.Name, d.Rule)).Should().Equal(("Two off", "2.00 EUR"));
    }

    [Fact]
    public async Task Should_SkipACouponInAnotherCurrency_AndStillPriceTheOrder()
    {
        using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "orders/discounts",
            new { customerId = 1005, price = 340m, currency = "USD", couponCodes = Welcome10 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        DiscountBreakdownResponse? breakdown = await response.Content.ReadFromJsonAsync<DiscountBreakdownResponse>();
        breakdown!.Discounts.Select(d => d.Source).Should().NotContain("Coupon");
        breakdown.FinalPrice.Should().Be(306.00m);
    }

    [Fact]
    public async Task Should_Return500_WhenAnEnteredCouponIsStoredInAnUnsupportedCurrency()
    {
        await ExecuteSqlAsync($"""
            INSERT INTO pricing.Coupons (Code, Name, Type, Value, Currency, ExpiresAtUtc, IsActive)
            VALUES ('YEN500', 'Yen coupon', 'FixedAmount', 500, 'JPY', NULL, 1)
            """);

        using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "orders/discounts",
            new { customerId = 1006, price = 340m, currency = "EUR", couponCodes = Yen500 });

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Should_IgnoreAnInactivePriceListRow_StoredInAnUnsupportedCurrency()
    {
        await ExecuteSqlAsync($"""
            INSERT INTO pricing.PriceListDiscounts (CustomerId, Name, Type, Value, Currency, IsActive)
            VALUES (1007, 'Old yen price list', 'FixedAmount', 500, 'JPY', 0),
                   (1007, 'Bronze price list', 'Percentage', 2, NULL, 1)
            """);

        DiscountBreakdownResponse breakdown = await CalculateAsync(customerId: 1007);

        breakdown.Discounts.Where(d => d.Source == "PriceList").Select(d => d.Name).Should().Equal("Bronze price list");
    }

    [Fact]
    public async Task Should_IgnoreAnExpiredCoupon_StoredInAnUnsupportedCurrency()
    {
        await ExecuteSqlAsync($"""
            INSERT INTO pricing.Coupons (Code, Name, Type, Value, Currency, ExpiresAtUtc, IsActive)
            VALUES ('OLDYEN', 'Expired yen coupon', 'FixedAmount', 500, 'JPY', {UtcNow.AddDays(-1)}, 1)
            """);

        DiscountBreakdownResponse breakdown = await CalculateAsync(customerId: 1008, "OLDYEN");

        breakdown.Discounts.Should().NotContain(d => d.Source == "Coupon");
    }

    [Fact]
    public async Task Should_KeepPricingOrders_WhenAPromotionCanNeverApply()
    {
        await ExecuteSqlAsync($"""
            INSERT INTO pricing.Promotions (Name, Type, Value, Currency, StartsAtUtc, EndsAtUtc, IsActive)
            VALUES ('Backwards promotion', 'Percentage', 20, NULL, {UtcNow.AddDays(2)}, {UtcNow.AddDays(1)}, 1),
                   ('Ended yen promotion', 'FixedAmount', 500, 'JPY', {UtcNow.AddDays(-10)}, {UtcNow.AddDays(-1)}, 1)
            """);

        DiscountBreakdownResponse breakdown = await CalculateAsync(customerId: 1009);

        breakdown.Discounts.Where(d => d.Source == "Promotion").Select(d => d.Name).Should().Equal("Autumn promotion");
    }
}
