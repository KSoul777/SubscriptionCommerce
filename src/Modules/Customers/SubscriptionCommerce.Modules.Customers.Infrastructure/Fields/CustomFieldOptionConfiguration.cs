using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Fields;

internal sealed class CustomFieldOptionConfiguration : IEntityTypeConfiguration<CustomFieldOption>
{
    internal const string UniqueLabelIndex = "IX_CustomFieldOptions_FieldDefinitionId_NormalizedLabel";

    public void Configure(EntityTypeBuilder<CustomFieldOption> builder)
    {
        builder.ToTable("CustomFieldOptions", table => table.HasCheckConstraint(
            "CK_CustomFieldOptions_Label_Canonical", CanonicalText.IsCanonical("Label")));
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Label).HasMaxLength(CustomFieldOption.LabelMaxLength);

        builder.Property<string>("NormalizedLabel")
            .IsRequired()
            .HasMaxLength(CustomFieldOption.LabelMaxLength)
            .HasComputedColumnSql(CanonicalText.KeyOf("Label"), stored: true);
        builder.HasIndex(nameof(CustomFieldOption.FieldDefinitionId), "NormalizedLabel").IsUnique().HasDatabaseName(UniqueLabelIndex);

        builder.HasAlternateKey(o => new { o.Id, o.FieldDefinitionId });
    }
}
