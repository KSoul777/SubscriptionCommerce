using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Coupons;

internal sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasMaxLength(50);
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(Discount.NameMaxLength);
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Value).HasPrecision(18, 2);
        builder.Property(c => c.Currency).HasMaxLength(3).IsFixedLength();

        // Example data from the brief: a coupon worth 10 EUR. Codes are stored upper-case.
        builder.HasData(new Coupon
        {
            Id = 1,
            Code = "WELCOME10",
            Name = "Welcome coupon",
            Type = DiscountType.FixedAmount,
            Value = 10m,
            Currency = "EUR",
            IsActive = true
        });
    }
}
