using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.FieldValues;

internal sealed class CustomerFieldValueRepository(CustomersDbContext context) : ICustomerFieldValueRepository
{
    public Task<CustomerFieldValue?> GetAsync(
        int customerId,
        int fieldDefinitionId,
        CancellationToken cancellationToken = default) =>
        context.FieldValues
            .Include(v => v.SelectedOptions)
            .SingleOrDefaultAsync(
                v => v.CustomerId == customerId && v.FieldDefinitionId == fieldDefinitionId,
                cancellationToken);

    public void Insert(CustomerFieldValue value)
    {
        context.FieldValues.Add(value);
    }
}
