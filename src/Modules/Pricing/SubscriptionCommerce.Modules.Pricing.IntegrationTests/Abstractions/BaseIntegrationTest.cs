using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts.CalculateOrderDiscounts;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Pricing.IntegrationTests.Abstractions;

// Pricing only reads its tables, so tests add their rows with plain SQL. Each test uses its own
// customer ids and coupon codes, and never adds an eligible promotion, so tests don't affect each other.
[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest(IntegrationTestWebAppFactory factory) : IDisposable
{
    protected HttpClient HttpClient { get; } = factory.CreateClient();

    protected async Task ExecuteSqlAsync(FormattableString sql)
    {
        using IServiceScope scope = factory.Services.CreateScope();

        PricingDbContext dbContext = scope.ServiceProvider.GetRequiredService<PricingDbContext>();

        await dbContext.Database.ExecuteSqlAsync(sql);
    }

    protected async Task<DiscountBreakdownResponse> CalculateAsync(int customerId, params string[] couponCodes)
    {
        using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "orders/discounts",
            new { customerId, price = 340m, currency = "EUR", couponCodes });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<DiscountBreakdownResponse>())!;
    }

    public void Dispose()
    {
        HttpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
