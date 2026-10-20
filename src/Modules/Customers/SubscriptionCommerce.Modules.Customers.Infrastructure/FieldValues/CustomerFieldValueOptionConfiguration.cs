using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.FieldValues;

internal sealed class CustomerFieldValueOptionConfiguration : IEntityTypeConfiguration<CustomerFieldValueOption>
{
    internal const string PrimaryKey = "PK_CustomerFieldValueOptions";

    public void Configure(EntityTypeBuilder<CustomerFieldValueOption> builder)
    {
        builder.ToTable("CustomerFieldValueOptions");
        builder.HasKey(o => new { o.CustomerFieldValueId, o.OptionId }).HasName(PrimaryKey);

        builder.HasOne<CustomFieldOption>()
            .WithMany()
            .HasForeignKey(o => new { o.OptionId, o.FieldDefinitionId })
            .HasPrincipalKey(o => new { o.Id, o.FieldDefinitionId })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
