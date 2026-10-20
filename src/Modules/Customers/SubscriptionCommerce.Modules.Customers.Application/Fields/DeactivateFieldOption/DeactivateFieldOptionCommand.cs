using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.DeactivateFieldOption;

public sealed record DeactivateFieldOptionCommand(int TenantId, int FieldId, int OptionId) : ICommand;
