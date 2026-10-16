using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Fields;

internal sealed class CustomFieldDefinitionRepository(CustomersDbContext context) : ICustomFieldDefinitionRepository
{
    public Task<CustomFieldDefinition?> GetAsync(int fieldId, CancellationToken cancellationToken = default) =>
        context.FieldDefinitions
            .Include(f => f.Options)
            .SingleOrDefaultAsync(f => f.Id == fieldId, cancellationToken);

    public async Task<IReadOnlyList<CustomFieldDefinition>> GetActiveForTenantAsync(
        int tenantId,
        CancellationToken cancellationToken = default) =>
        await context.FieldDefinitions
            .AsNoTracking()
            .Include(f => f.Options)
            .Where(f => f.TenantId == tenantId && f.IsActive)
            .ToListAsync(cancellationToken);

    public Task<bool> NameExistsAsync(int tenantId, string name, CancellationToken cancellationToken = default) =>
        context.FieldDefinitions.AnyAsync(f => f.TenantId == tenantId && f.Name == name, cancellationToken);

    public void Insert(CustomFieldDefinition field)
    {
        context.FieldDefinitions.Add(field);
    }
}
