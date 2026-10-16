using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.CreateField;

public sealed record CreateFieldCommand(
    int TenantId,
    string Name,
    FieldType Type,
    bool AllowMultipleSelection,
    bool IsRequired,
    int SortOrder) : ICommand<int>;
