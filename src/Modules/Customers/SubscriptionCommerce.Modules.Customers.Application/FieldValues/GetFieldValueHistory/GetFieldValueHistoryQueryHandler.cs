using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;

internal sealed class GetFieldValueHistoryQueryHandler(IFieldValueHistoryReader reader)
    : IQueryHandler<GetFieldValueHistoryQuery, FieldValueHistoryPage>
{
    public async Task<Result<FieldValueHistoryPage>> Handle(
        GetFieldValueHistoryQuery query,
        CancellationToken cancellationToken)
    {
        FieldValueHistoryPage? page = await reader.GetPageAsync(
            query.TenantId, query.CustomerId, query.FieldId, query.After, query.Limit, cancellationToken);

        return page is null
            ? Result.Failure<FieldValueHistoryPage>(CustomerFieldValueErrors.NotFound(query.CustomerId, query.FieldId))
            : Result.Success(page);
    }
}
