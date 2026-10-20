using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;

public sealed record GetCustomerProfileQuery(int TenantId, int CustomerId) : IQuery<CustomerProfileResponse>;

public sealed record CustomerProfileResponse(
    int CustomerId,
    int TenantId,
    string Name,
    IReadOnlyList<ProfileFieldResponse> Fields);

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
