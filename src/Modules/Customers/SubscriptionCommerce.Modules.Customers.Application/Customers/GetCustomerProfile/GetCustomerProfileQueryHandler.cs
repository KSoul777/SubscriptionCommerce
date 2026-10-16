using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;

// Every active field of the customer's tenant, with the customer's value where there is one.
internal sealed class GetCustomerProfileQueryHandler(
    ICustomerRepository customerRepository,
    ICustomFieldDefinitionRepository fieldRepository,
    ICustomerFieldValueRepository valueRepository)
    : IQueryHandler<GetCustomerProfileQuery, CustomerProfileResponse>
{
    public async Task<Result<CustomerProfileResponse>> Handle(
        GetCustomerProfileQuery query,
        CancellationToken cancellationToken)
    {
        Customer? customer = await customerRepository.GetAsync(query.CustomerId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerProfileResponse>(CustomerErrors.NotFound(query.CustomerId));
        }

        IReadOnlyList<CustomFieldDefinition> fields =
            await fieldRepository.GetActiveForTenantAsync(customer.TenantId, cancellationToken);

        var values =
            (await valueRepository.GetForCustomerAsync(customer.Id, cancellationToken))
            .ToDictionary(v => v.FieldDefinitionId);

        var profileFields = fields
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Id)
            .Select(field => ToResponse(field, values.GetValueOrDefault(field.Id)))
            .ToList();

        return new CustomerProfileResponse(customer.Id, customer.TenantId, customer.Name, profileFields);
    }

    private static ProfileFieldResponse ToResponse(CustomFieldDefinition field, CustomerFieldValue? value)
    {
        CustomFieldOption[] options = field.Options.OrderBy(o => o.SortOrder).ThenBy(o => o.Id).ToArray();
        var selectedIds = (value?.SelectedOptions.Select(o => o.OptionId) ?? []).ToHashSet();

        return new ProfileFieldResponse(
            field.Id,
            field.Name,
            field.Type.ToString(),
            field.AllowMultipleSelection,
            field.IsRequired,
            value?.TextValue,
            // A selected option stays visible even after it was deactivated.
            options.Where(o => selectedIds.Contains(o.Id)).Select(o => new OptionResponse(o.Id, o.Label)).ToList(),
            options.Where(o => o.IsActive).Select(o => new OptionResponse(o.Id, o.Label)).ToList(),
            value is null ? null : Convert.ToBase64String(value.RowVersion));
    }
}
