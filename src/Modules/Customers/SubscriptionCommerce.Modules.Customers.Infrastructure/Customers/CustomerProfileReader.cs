using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Customers;

internal sealed class CustomerProfileReader(CustomersDbContext context) : ICustomerProfileReader
{
    public async Task<CustomerProfileResponse?> GetAsync(int tenantId, int customerId, CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .Where(c => c.Id == customerId && c.TenantId == tenantId)
            .Select(c => new { c.Id, c.TenantId, c.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return null;
        }

        List<CustomFieldDefinition> fields = await context.FieldDefinitions
            .AsNoTracking()
            .Include(f => f.Options)
            .Where(f => f.TenantId == tenantId && f.IsActive)
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Id)
            .ToListAsync(cancellationToken);

        Dictionary<int, CustomerFieldValue> values = await context.FieldValues
            .AsNoTracking()
            .Include(v => v.SelectedOptions)
            .Where(v => v.CustomerId == customerId)
            .ToDictionaryAsync(v => v.FieldDefinitionId, cancellationToken);

        return new CustomerProfileResponse(
            customer.Id,
            customer.TenantId,
            customer.Name,
            fields.Select(field => ToResponse(field, values.GetValueOrDefault(field.Id))).ToList());
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
            options.Where(o => selectedIds.Contains(o.Id)).Select(o => new OptionResponse(o.Id, o.Label)).ToList(),
            options.Where(o => o.IsActive).Select(o => new OptionResponse(o.Id, o.Label)).ToList(),
            value is null ? null : Convert.ToBase64String(value.RowVersion));
    }
}
