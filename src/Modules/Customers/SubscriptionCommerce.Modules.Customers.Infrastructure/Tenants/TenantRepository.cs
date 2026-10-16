using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Tenants;

internal sealed class TenantRepository(CustomersDbContext context) : ITenantRepository
{
    public Task<bool> ExistsAsync(int tenantId, CancellationToken cancellationToken = default) =>
        context.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken);

    public void Insert(Tenant tenant)
    {
        context.Tenants.Add(tenant);
    }
}
