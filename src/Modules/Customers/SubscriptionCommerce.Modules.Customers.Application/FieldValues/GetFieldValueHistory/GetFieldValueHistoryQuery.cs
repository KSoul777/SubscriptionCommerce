using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;

public sealed record GetFieldValueHistoryQuery(int TenantId, int CustomerId, int FieldId, long? After, int Limit)
    : IQuery<FieldValueHistoryPage>
{
    public const int DefaultLimit = 50;

    public const int MaxLimit = 200;
}

public sealed record FieldValueHistoryPage(IReadOnlyList<FieldValueHistoryResponse> Items, long? NextAfter);

public sealed record FieldValueHistoryResponse(
    long Id,
    FieldValueSnapshot? OldValue,
    FieldValueSnapshot NewValue,
    DateTime ChangedAtUtc,
    string ChangedBy);
