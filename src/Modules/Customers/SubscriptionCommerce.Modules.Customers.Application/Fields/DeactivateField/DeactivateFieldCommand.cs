using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.DeactivateField;

public sealed record DeactivateFieldCommand(int TenantId, int FieldId) : ICommand;
