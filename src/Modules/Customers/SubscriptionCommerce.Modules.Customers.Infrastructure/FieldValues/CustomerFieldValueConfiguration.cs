using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.FieldValues;

internal sealed class CustomerFieldValueConfiguration : IEntityTypeConfiguration<CustomerFieldValue>
{
    public void Configure(EntityTypeBuilder<CustomerFieldValue> builder)
    {
        builder.ToTable("CustomerFieldValues");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.TextValue).HasMaxLength(CustomerFieldValue.TextMaxLength);
        builder.Property(v => v.UpdatedBy).HasMaxLength(200);
        builder.Property(v => v.RowVersion).IsRowVersion();

        // One value row per customer and field. Two concurrent first saves: the second hits this index.
        builder.HasIndex(v => new { v.CustomerId, v.FieldDefinitionId }).IsUnique();

        builder.HasOne<Customer>().WithMany().HasForeignKey(v => v.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CustomFieldDefinition>().WithMany().HasForeignKey(v => v.FieldDefinitionId).OnDelete(DeleteBehavior.Restrict);

        // Selected options are replaced as the value changes.
        builder.HasMany(v => v.SelectedOptions)
            .WithOne()
            .HasForeignKey(o => o.CustomerFieldValueId)
            .OnDelete(DeleteBehavior.Cascade);

        // History is append-only.
        builder.HasMany(v => v.History)
            .WithOne()
            .HasForeignKey(h => h.CustomerFieldValueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
