using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Fields;

internal sealed class CustomFieldOptionConfiguration : IEntityTypeConfiguration<CustomFieldOption>
{
    public void Configure(EntityTypeBuilder<CustomFieldOption> builder)
    {
        builder.ToTable("CustomFieldOptions");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Label).HasMaxLength(CustomFieldOption.LabelMaxLength);

        // The default SQL Server collation is case-insensitive, matching the domain's duplicate-label check.
        builder.HasIndex(o => new { o.FieldDefinitionId, o.Label }).IsUnique();
    }
}
