using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;
using SubscriptionCommerce.Modules.Customers.Domain.Users;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Fields;

internal sealed class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    internal const string NormalizedName = nameof(NormalizedName);

    internal const string UniqueNameIndex = "IX_CustomFieldDefinitions_TenantId_NormalizedName";

    public void Configure(EntityTypeBuilder<CustomFieldDefinition> builder)
    {
        builder.ToTable("CustomFieldDefinitions", table => table.HasCheckConstraint(
            "CK_CustomFieldDefinitions_Name_Canonical", CanonicalText.IsCanonical("Name")));
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Name).HasMaxLength(CustomFieldDefinition.NameMaxLength);
        builder.Property(f => f.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.CreatedBy).HasMaxLength(UserName.MaxLength);
        builder.Property<string>(NormalizedName)
            .IsRequired()
            .HasMaxLength(CustomFieldDefinition.NameMaxLength)
            .HasComputedColumnSql(CanonicalText.KeyOf("Name"), stored: true);
        builder.HasIndex(nameof(CustomFieldDefinition.TenantId), NormalizedName).IsUnique().HasDatabaseName(UniqueNameIndex);
        builder.HasOne<Tenant>().WithMany().HasForeignKey(f => f.TenantId).OnDelete(DeleteBehavior.Restrict);

        builder.HasAlternateKey(f => new { f.Id, f.TenantId });

        builder.HasMany(f => f.Options)
            .WithOne()
            .HasForeignKey(o => o.FieldDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(f => f.Options).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
