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

    public async Task<IReadOnlyList<CustomerFieldValue>> GetForCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default) =>
        await context.FieldValues
            .AsNoTracking()
            .Include(v => v.SelectedOptions)
            .Where(v => v.CustomerId == customerId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CustomerFieldValueHistory>> GetHistoryAsync(
        int customerId,
        int fieldDefinitionId,
        CancellationToken cancellationToken = default) =>
        await context.FieldValueHistory
            .AsNoTracking()
            .Where(h => context.FieldValues.Any(v =>
                v.Id == h.CustomerFieldValueId &&
                v.CustomerId == customerId &&
                v.FieldDefinitionId == fieldDefinitionId))
            .OrderBy(h => h.Id)
            .ToListAsync(cancellationToken);

    public void Insert(CustomerFieldValue value)
    {
        context.FieldValues.Add(value);
    }
}
