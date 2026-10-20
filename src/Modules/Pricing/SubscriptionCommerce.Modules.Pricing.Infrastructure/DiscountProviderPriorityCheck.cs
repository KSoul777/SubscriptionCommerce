using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure;

internal sealed class DiscountProviderPriorityCheck(IServiceScopeFactory scopeFactory) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();

        string[] shared = scope.ServiceProvider.GetServices<IDiscountProvider>()
            .GroupBy(p => p.Priority)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key} ({string.Join(", ", g.Select(p => p.GetType().Name))})")
            .ToArray();

        if (shared.Length > 0)
        {
            throw new InvalidOperationException(
                $"Every discount provider needs its own priority. Shared priorities: {string.Join("; ", shared)}");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
