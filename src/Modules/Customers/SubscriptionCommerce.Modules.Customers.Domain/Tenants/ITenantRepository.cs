namespace SubscriptionCommerce.Modules.Customers.Domain.Tenants;

public interface ITenantRepository
{
    Task<bool> ExistsAsync(int tenantId, CancellationToken cancellationToken = default);

    void Insert(Tenant tenant);
}
