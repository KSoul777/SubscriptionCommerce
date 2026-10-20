using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Pricing.Domain.Coupons;
using SubscriptionCommerce.Modules.Pricing.Domain.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Domain.Promotions;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Coupons;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Promotions;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

public sealed class PricingDbContext(DbContextOptions<PricingDbContext> options) : DbContext(options)
{
    internal DbSet<PriceListDiscount> PriceListDiscounts => Set<PriceListDiscount>();

    internal DbSet<Promotion> Promotions => Set<Promotion>();

    internal DbSet<Coupon> Coupons => Set<Coupon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Pricing);

        modelBuilder.ApplyConfiguration(new PriceListDiscountConfiguration());
        modelBuilder.ApplyConfiguration(new PromotionConfiguration());
        modelBuilder.ApplyConfiguration(new CouponConfiguration());
    }
}
