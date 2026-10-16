using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.FieldValues;

internal sealed class CustomerFieldValueOptionConfiguration : IEntityTypeConfiguration<CustomerFieldValueOption>
{
    public void Configure(EntityTypeBuilder<CustomerFieldValueOption> builder)
    {
        builder.ToTable("CustomerFieldValueOptions");
        builder.HasKey(o => new { o.CustomerFieldValueId, o.OptionId });
        builder.HasOne<CustomFieldOption>().WithMany().HasForeignKey(o => o.OptionId).OnDelete(DeleteBehavior.Restrict);
    }
}
