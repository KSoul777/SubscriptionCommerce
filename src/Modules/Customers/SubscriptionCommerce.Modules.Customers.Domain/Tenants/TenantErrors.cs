using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Customers.Domain.Tenants;

public static class TenantErrors
{
    public static Error NotFound(int tenantId) =>
        Error.NotFound("Tenants.NotFound", $"The tenant with the identifier {tenantId} was not found");
}
