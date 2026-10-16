using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;

// RowVersion: the base64 row version the caller last read, or null when the customer has no value yet.
public sealed record SetCustomerFieldValueCommand(
    int CustomerId,
    int FieldId,
    string? TextValue,
    IReadOnlyCollection<int> SelectedOptionIds,
    string? RowVersion) : ICommand<SetCustomerFieldValueResponse>;

public sealed record SetCustomerFieldValueResponse(bool Changed, string? RowVersion);
