using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Fields;

internal sealed class CustomFieldDefinitionRepository(CustomersDbContext context) : ICustomFieldDefinitionRepository
{
    public Task<CustomFieldDefinition?> GetAsync(int tenantId, int fieldId, CancellationToken cancellationToken = default) =>
        context.FieldDefinitions
            .Include(f => f.Options)
            .SingleOrDefaultAsync(f => f.Id == fieldId && f.TenantId == tenantId, cancellationToken);

    public Task<bool> NameExistsAsync(int tenantId, string name, CancellationToken cancellationToken = default)
    {
        string key = UniqueKey.For(name);

        return context.FieldDefinitions.AnyAsync(
            f => f.TenantId == tenantId && EF.Property<string>(f, CustomFieldDefinitionConfiguration.NormalizedName) == key,
            cancellationToken);
    }

    public void Insert(CustomFieldDefinition field)
    {
        context.FieldDefinitions.Add(field);
    }
}
