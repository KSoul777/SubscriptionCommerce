using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Fields;

internal sealed class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CustomFieldDefinition> builder)
    {
        builder.ToTable("CustomFieldDefinitions");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Name).HasMaxLength(CustomFieldDefinition.NameMaxLength);
        builder.Property(f => f.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.CreatedBy).HasMaxLength(200);
        builder.HasIndex(f => new { f.TenantId, f.Name }).IsUnique();
        builder.HasOne<Tenant>().WithMany().HasForeignKey(f => f.TenantId).OnDelete(DeleteBehavior.Restrict);

        // Restrict: fields and options are deactivated, never deleted.
        builder.HasMany(f => f.Options)
            .WithOne()
            .HasForeignKey(o => o.FieldDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
