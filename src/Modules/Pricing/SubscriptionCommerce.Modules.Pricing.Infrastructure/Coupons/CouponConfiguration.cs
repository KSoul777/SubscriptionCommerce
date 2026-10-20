using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Coupons;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Coupons;

internal sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasMaxLength(Coupon.CodeMaxLength);
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(Discount.NameMaxLength);
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Value).HasPrecision(18, Currency.DecimalPlaces);
        builder.Property(c => c.Currency).HasConversion<CurrencyConverter>().HasMaxLength(3).IsFixedLength();

        builder.HasData(new
        {
            Id = 1,
            Code = "WELCOME10",
            Name = "Welcome coupon",
            Type = DiscountType.FixedAmount,
            Value = 10m,
            Currency = Currency.Create("EUR").Value,
            IsActive = true
        });
    }
}
