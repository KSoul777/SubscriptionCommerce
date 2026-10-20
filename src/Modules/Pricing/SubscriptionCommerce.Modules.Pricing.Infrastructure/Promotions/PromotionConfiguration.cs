using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.Promotions;
using SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Promotions;

internal sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(Discount.NameMaxLength);
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Value).HasPrecision(18, Currency.DecimalPlaces);
        builder.Property(p => p.Currency).HasConversion<CurrencyConverter>().HasMaxLength(3).IsFixedLength();

        builder.HasData(new
        {
            Id = 1,
            Name = "Autumn promotion",
            Type = DiscountType.Percentage,
            Value = 10m,
            StartsAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndsAtUtc = new DateTime(2031, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        });
    }
}
