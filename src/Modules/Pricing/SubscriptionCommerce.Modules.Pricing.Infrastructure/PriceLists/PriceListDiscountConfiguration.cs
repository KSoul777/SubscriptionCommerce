using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.PriceLists;

internal sealed class PriceListDiscountConfiguration : IEntityTypeConfiguration<PriceListDiscount>
{
    public void Configure(EntityTypeBuilder<PriceListDiscount> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).HasMaxLength(Discount.NameMaxLength);
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Value).HasPrecision(18, 2);
        builder.Property(d => d.Currency).HasMaxLength(3).IsFixedLength();
        builder.HasIndex(d => d.CustomerId);

        // Example data from the brief: customer 1 gets 5% from their price list.
        builder.HasData(new PriceListDiscount
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
