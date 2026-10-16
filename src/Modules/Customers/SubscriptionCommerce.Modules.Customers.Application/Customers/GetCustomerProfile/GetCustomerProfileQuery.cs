using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;

public sealed record GetCustomerProfileQuery(int CustomerId) : IQuery<CustomerProfileResponse>;

public sealed record CustomerProfileResponse(
    int CustomerId,
    int TenantId,
    string Name,
    IReadOnlyList<ProfileFieldResponse> Fields);

// RowVersion is null until the customer has a value for the field; send it back when setting the value.
public sealed record ProfileFieldResponse(
    int FieldId,
    string Name,
    string Type,
    bool AllowMultipleSelection,
    bool IsRequired,
    string? TextValue,
    IReadOnlyList<OptionResponse> SelectedOptions,
    IReadOnlyList<OptionResponse> AvailableOptions,
    string? RowVersion);

public sealed record OptionResponse(int Id, string Label);
