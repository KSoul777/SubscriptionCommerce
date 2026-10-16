using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.RenameFieldOption;

public sealed record RenameFieldOptionCommand(int FieldId, int OptionId, string Label) : ICommand;
