using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Application.Coupons;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Application.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Application.Promotions;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using SubscriptionCommerce.Modules.Pricing.Infrastructure;

namespace SubscriptionCommerce.Modules.Pricing.UnitTests.Providers;

public class ProviderRegistrationTests
{
    [Fact]
    public void AddPricingModule_Should_RegisterEveryDiscountProviderWithAUniquePriority()
    {
        using ServiceProvider serviceProvider = PricingServices().BuildServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();

        IDiscountProvider[] providers = scope.ServiceProvider.GetServices<IDiscountProvider>().ToArray();

        providers.Select(p => p.GetType()).Should().BeEquivalentTo(
            [typeof(PriceListDiscountProvider), typeof(PromotionDiscountProvider), typeof(CouponDiscountProvider)]);
        providers.Select(p => p.Priority).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task Startup_Should_Succeed_WithTheRegisteredProviders()
    {
        using ServiceProvider serviceProvider = PricingServices().BuildServiceProvider();

        Func<Task> start = () => serviceProvider.GetServices<IHostedService>().Single().StartAsync(CancellationToken.None);

        await start.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Startup_Should_Fail_WhenTwoProvidersShareAPriority()
    {
        ServiceCollection services = PricingServices();
        services.AddScoped<IDiscountProvider, SecondCouponSource>();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        Func<Task> start = () => serviceProvider.GetServices<IHostedService>().Single().StartAsync(CancellationToken.None);

        await start.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*300 (CouponDiscountProvider, SecondCouponSource)*");
    }

    private static ServiceCollection PricingServices()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Database"] = "Server=unused" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddLogging();
        services.AddPricingModule(configuration);

        return services;
    }

    private sealed class SecondCouponSource : IDiscountProvider
    {
        public int Priority => 300;

        public Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(Order order, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success<IReadOnlyList<Discount>>([]));
    }
}
