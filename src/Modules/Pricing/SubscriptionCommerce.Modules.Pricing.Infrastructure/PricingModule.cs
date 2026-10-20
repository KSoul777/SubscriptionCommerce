using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionCommerce.Common.Infrastructure.Configuration;
using SubscriptionCommerce.Modules.Pricing.Application.Coupons;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Application.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Application.Promotions;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Coupons;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Promotions;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure;

public static class PricingModule
{
    public static IServiceCollection AddPricingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddDiscountProviders();

        services.AddHostedService<DiscountProviderPriorityCheck>();

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PricingDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionStringOrThrow("Database"),
                sqlOptions => sqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Pricing)));

        services.AddScoped<IPriceListReader, PriceListReader>();
        services.AddScoped<IPromotionReader, PromotionReader>();
        services.AddScoped<ICouponReader, CouponReader>();
    }

    private static void AddDiscountProviders(this IServiceCollection services) =>
        services.Scan(scan => scan.FromAssemblies(Application.AssemblyReference.Assembly)
            .AddClasses(classes => classes.AssignableTo<IDiscountProvider>(), publicOnly: false)
                .As<IDiscountProvider>()
                .WithScopedLifetime());
}
