using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.Domain.Users;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.FieldValues;

internal sealed class CustomerFieldValueConfiguration : IEntityTypeConfiguration<CustomerFieldValue>
{
    internal const string OneValuePerFieldIndex = "IX_CustomerFieldValues_CustomerId_FieldDefinitionId";

    public void Configure(EntityTypeBuilder<CustomerFieldValue> builder)
    {
        builder.ToTable("CustomerFieldValues");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.TextValue).HasMaxLength(CustomerFieldValue.TextMaxLength);
        builder.Property(v => v.UpdatedBy).HasMaxLength(UserName.MaxLength);
        builder.Property(v => v.RowVersion).IsRowVersion();

        builder.HasIndex(v => new { v.CustomerId, v.FieldDefinitionId }).IsUnique().HasDatabaseName(OneValuePerFieldIndex);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(v => new { v.CustomerId, v.TenantId })
            .HasPrincipalKey(c => new { c.Id, c.TenantId })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CustomFieldDefinition>()
            .WithMany()
            .HasForeignKey(v => new { v.FieldDefinitionId, v.TenantId })
            .HasPrincipalKey(f => new { f.Id, f.TenantId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasAlternateKey(v => new { v.Id, v.FieldDefinitionId });

        builder.HasMany(v => v.SelectedOptions)
            .WithOne()
            .HasForeignKey(o => new { o.CustomerFieldValueId, o.FieldDefinitionId })
            .HasPrincipalKey(v => new { v.Id, v.FieldDefinitionId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.PendingHistory)
            .WithOne()
            .HasForeignKey(h => h.CustomerFieldValueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(v => v.SelectedOptions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(v => v.PendingHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
