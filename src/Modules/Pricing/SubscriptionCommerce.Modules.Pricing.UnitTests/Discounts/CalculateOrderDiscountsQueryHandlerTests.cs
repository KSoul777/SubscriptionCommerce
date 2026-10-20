using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts.CalculateOrderDiscounts;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using static SubscriptionCommerce.Modules.Pricing.UnitTests.Abstractions.TestData;

namespace SubscriptionCommerce.Modules.Pricing.UnitTests.Discounts;

public class CalculateOrderDiscountsQueryHandlerTests
{
    private static readonly CalculateOrderDiscountsQuery Query = new(CustomerId: 1, Price: 340m, Currency: "EUR", CouponCodes: []);

    [Fact]
    public async Task Handle_Should_ApplyProvidersInPriorityOrder_RegardlessOfRegistrationOrder()
    {
        var handler = new CalculateOrderDiscountsQueryHandler(
        [
            new StubProvider(300, FixedAmount("Coupon", 10m)),
            new StubProvider(200, Percentage("Promotion", 10m)),
            new StubProvider(100, Percentage("Price list", 5m))
        ]);

        Result<DiscountBreakdownResponse> result = await handler.Handle(Query, CancellationToken.None);

        result.Value.Discounts.Select(d => d.Name).Should().Equal("Price list", "Promotion", "Coupon");
        result.Value.FinalPrice.Should().Be(280.70m);
    }

    [Fact]
    public async Task Handle_Should_KeepTheProvidersOwnOrder_WhenOneSourceReturnsSeveralDiscounts()
    {
        var handler = new CalculateOrderDiscountsQueryHandler(
        [
            new StubProvider(200, Percentage("Promotion B", 10m), Percentage("Promotion A", 20m)),
            new StubProvider(100, Percentage("Price list", 5m))
        ]);

        Result<DiscountBreakdownResponse> result = await handler.Handle(Query, CancellationToken.None);

        result.Value.Discounts.Select(d => d.Name).Should().Equal("Price list", "Promotion B", "Promotion A");
    }

    [Fact]
    public async Task Handle_Should_ApplyANewSource_WithoutChangingTheHandler()
    {
        var handler = new CalculateOrderDiscountsQueryHandler(
        [
            new StubProvider(100, Percentage("Price list", 5m)),
            new StubProvider(200, Percentage("Promotion", 10m)),
            new LoyaltyDiscountProvider()
        ]);

        Result<DiscountBreakdownResponse> result = await handler.Handle(Query, CancellationToken.None);

        result.Value.Discounts.Select(d => d.Source).Should().Equal("Test", "Loyalty", "Test");
        result.Value.Discounts[1].Deducted.Should().Be(6.46m);
    }

    [Fact]
    public async Task Handle_Should_SkipFixedAmountsInAnotherCurrency_AndApplyTheRest()
    {
        var handler = new CalculateOrderDiscountsQueryHandler(
        [
            new StubProvider(100, Percentage("Price list", 5m)),
            new StubProvider(300, FixedAmount("Dollar coupon", 10m, "USD"), FixedAmount("Euro coupon", 10m))
        ]);

        Result<DiscountBreakdownResponse> result = await handler.Handle(Query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Discounts.Select(d => d.Name).Should().Equal("Price list", "Euro coupon");
        result.Value.FinalPrice.Should().Be(313.00m);
    }

    [Fact]
    public async Task Handle_Should_FailWithoutAPartialPrice_WhenAProviderFails()
    {
        var error = Error.Failure("Promotions.Unavailable", "The promotions service is unavailable");
        var later = new StubProvider(300, FixedAmount("Coupon", 10m));
        var handler = new CalculateOrderDiscountsQueryHandler(
        [
            new StubProvider(100, Percentage("Price list", 5m)),
            new FailingProvider(200, error),
            later
        ]);

        Result<DiscountBreakdownResponse> result = await handler.Handle(Query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        later.ReceivedOrder.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_LetExceptionsPropagate_WhenAProviderThrows()
    {
        var handler = new CalculateOrderDiscountsQueryHandler([new ThrowingProvider()]);

        Func<Task> act = () => handler.Handle(Query, CancellationToken.None);

        await act.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task Handle_Should_PassNormalizedCouponCodesToProviders()
    {
        var provider = new StubProvider(300);
        var handler = new CalculateOrderDiscountsQueryHandler([provider]);

        await handler.Handle(Query with { CouponCodes = [" welcome10 ", "WELCOME10", "Spring"] }, CancellationToken.None);

        provider.ReceivedOrder!.CouponCodes.Should().Equal("WELCOME10", "SPRING");
    }

    [Fact]
    public async Task Handle_Should_Fail_WhenTheCurrencyIsInvalid()
    {
        var handler = new CalculateOrderDiscountsQueryHandler([]);

        Result<DiscountBreakdownResponse> result = await handler.Handle(Query with { Currency = "12" }, CancellationToken.None);

        result.Error.Should().Be(MoneyErrors.CurrencyInvalid);
    }

    private sealed class StubProvider(int priority, params Discount[] discounts) : IDiscountProvider
    {
        public int Priority => priority;

        public Order? ReceivedOrder { get; private set; }

        public Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(Order order, CancellationToken cancellationToken)
        {
            ReceivedOrder = order;
            return Task.FromResult(Result.Success<IReadOnlyList<Discount>>(discounts));
        }
    }

    private sealed class LoyaltyDiscountProvider : IDiscountProvider
    {
        public int Priority => 150;

        public Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(Order order, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success<IReadOnlyList<Discount>>([Percentage("Loyal customer", 2m, source: "Loyalty")]));
    }

    private sealed class FailingProvider(int priority, Error error) : IDiscountProvider
    {
        public int Priority => priority;

        public Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(Order order, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Failure<IReadOnlyList<Discount>>(error));
    }

    private sealed class ThrowingProvider : IDiscountProvider
    {
        public int Priority => 100;

        public Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(Order order, CancellationToken cancellationToken) =>
            throw new TimeoutException("Promotions service timed out");
    }
}
