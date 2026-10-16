using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.AddFieldOption;

public sealed record AddFieldOptionCommand(int FieldId, string Label, int SortOrder) : ICommand<int>;
