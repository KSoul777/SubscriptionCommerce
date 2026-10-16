using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;

internal sealed class GetFieldValueHistoryQueryHandler(
    ICustomerRepository customerRepository,
    ICustomFieldDefinitionRepository fieldRepository,
    ICustomerFieldValueRepository valueRepository)
    : IQueryHandler<GetFieldValueHistoryQuery, IReadOnlyList<FieldValueHistoryResponse>>
{
    public async Task<Result<IReadOnlyList<FieldValueHistoryResponse>>> Handle(
        GetFieldValueHistoryQuery query,
        CancellationToken cancellationToken)
    {
        Customer? customer = await customerRepository.GetAsync(query.CustomerId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<IReadOnlyList<FieldValueHistoryResponse>>(CustomerErrors.NotFound(query.CustomerId));
        }

        CustomFieldDefinition? field = await fieldRepository.GetAsync(query.FieldId, cancellationToken);

        if (field is null || field.TenantId != customer.TenantId)
        {
            return Result.Failure<IReadOnlyList<FieldValueHistoryResponse>>(CustomFieldErrors.NotFound(query.FieldId));
        }

        IReadOnlyList<CustomerFieldValueHistory> history =
            await valueRepository.GetHistoryAsync(customer.Id, field.Id, cancellationToken);

        return history
            .Select(h => new FieldValueHistoryResponse(h.Id, h.OldValue, h.NewValue, h.ChangedAtUtc, h.ChangedBy))
            .ToList();
    }
}
