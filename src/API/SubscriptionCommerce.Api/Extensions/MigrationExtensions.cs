using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Api.Extensions;

internal static class MigrationExtensions
{
    // Development only: production would run migrations as a deployment step, not on application start.
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        ApplyMigration<PricingDbContext>(scope);
        ApplyMigration<CustomersDbContext>(scope);
    }

    private static void ApplyMigration<TDbContext>(IServiceScope scope)
        where TDbContext : DbContext
    {
        using TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        context.Database.Migrate();
    }
}
