using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.PriceLists;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.PriceLists;

internal sealed class PriceListDiscountConfiguration : IEntityTypeConfiguration<PriceListDiscount>
{
    public void Configure(EntityTypeBuilder<PriceListDiscount> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).HasMaxLength(Discount.NameMaxLength);
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Value).HasPrecision(18, Currency.DecimalPlaces);
        builder.Property(d => d.Currency).HasConversion<CurrencyConverter>().HasMaxLength(3).IsFixedLength();
        builder.HasIndex(d => d.CustomerId);

        builder.HasData(new
        {
            Id = 1,
            CustomerId = 1,
            Name = "Gold price list",
            Type = DiscountType.Percentage,
            Value = 5m,
            IsActive = true
        });
    }
}
