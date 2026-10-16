using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionCommerce.Modules.Pricing.Application.Coupons;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Application.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Application.Promotions;
using SubscriptionCommerce.Modules.Pricing.Infrastructure;

namespace SubscriptionCommerce.Modules.Pricing.UnitTests.Providers;

public class ProviderRegistrationTests
{
    [Fact]
    public void AddPricingModule_Should_RegisterEveryDiscountProviderWithAUniquePriority()
    {
        // Arrange: the connection string is never opened; resolving providers only builds the DbContext.
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Database"] = "Server=unused" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddPricingModule(configuration);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();

        // Act
        IDiscountProvider[] providers = scope.ServiceProvider.GetServices<IDiscountProvider>().ToArray();

        // Assert
        providers.Select(p => p.GetType()).Should().BeEquivalentTo(
            [typeof(PriceListDiscountProvider), typeof(PromotionDiscountProvider), typeof(CouponDiscountProvider)]);
        providers.Select(p => p.Priority).Should().OnlyHaveUniqueItems();
    }
}
