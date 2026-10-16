using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.FieldValues;

internal sealed class CustomerFieldValueHistoryConfiguration : IEntityTypeConfiguration<CustomerFieldValueHistory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<CustomerFieldValueHistory> builder)
    {
        builder.ToTable("CustomerFieldValueHistory");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.ChangedBy).HasMaxLength(200);

        // Snapshots are stored as JSON: they are only ever written once and read back whole.
        builder.Property(h => h.OldValue)
            .HasConversion(
                snapshot => JsonSerializer.Serialize(snapshot, JsonOptions),
                json => JsonSerializer.Deserialize<FieldValueSnapshot>(json, JsonOptions));

        builder.Property(h => h.NewValue)
            .HasConversion(
                snapshot => JsonSerializer.Serialize(snapshot, JsonOptions),
                json => JsonSerializer.Deserialize<FieldValueSnapshot>(json, JsonOptions)!);
    }
}
