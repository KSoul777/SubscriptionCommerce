using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;

public sealed record SetCustomerFieldValueCommand(
    int TenantId,
    int CustomerId,
    int FieldId,
    string? TextValue,
    IReadOnlyCollection<int> SelectedOptionIds,
    string? RowVersion) : ICommand<SetCustomerFieldValueResponse>;

public sealed record SetCustomerFieldValueResponse(bool Changed, string? RowVersion);
