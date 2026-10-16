using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Customers.Application.Tenants.CreateTenant;

public sealed record CreateTenantCommand(string Name) : ICommand<int>;
