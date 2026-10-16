using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Customers.Application.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(int TenantId, string Name) : ICommand<int>;
