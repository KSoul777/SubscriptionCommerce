using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Coupons;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Promotions;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

// Read-only from this module's point of view: price lists, promotions and coupons are maintained elsewhere
// in the ERP. The tables here stand in for the data the stored procedure used to query.
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
