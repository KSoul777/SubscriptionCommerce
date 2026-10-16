using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;

public sealed record GetFieldValueHistoryQuery(int CustomerId, int FieldId) : IQuery<IReadOnlyList<FieldValueHistoryResponse>>;

public sealed record FieldValueHistoryResponse(
    long Id,
    FieldValueSnapshot? OldValue,
    FieldValueSnapshot NewValue,
    DateTime ChangedAtUtc,
    string ChangedBy);
