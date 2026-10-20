using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.FieldValues;

internal sealed class FieldValueHistoryReader(CustomersDbContext context) : IFieldValueHistoryReader
{
    public async Task<FieldValueHistoryPage?> GetPageAsync(
        int tenantId,
        int customerId,
        int fieldId,
        long? after,
        int limit,
        CancellationToken cancellationToken)
    {
        bool customerInTenant = await context.Customers
            .AnyAsync(c => c.Id == customerId && c.TenantId == tenantId, cancellationToken);
        bool fieldInTenant = await context.FieldDefinitions
            .AnyAsync(f => f.Id == fieldId && f.TenantId == tenantId, cancellationToken);

        if (!customerInTenant || !fieldInTenant)
        {
            return null;
        }

        long afterId = after ?? 0;

        List<FieldValueHistoryResponse> rows = await context.FieldValueHistory
            .AsNoTracking()
            .Where(h => h.Id > afterId && context.FieldValues.Any(v =>
                v.Id == h.CustomerFieldValueId &&
                v.CustomerId == customerId &&
                v.FieldDefinitionId == fieldId))
            .OrderBy(h => h.Id)
            .Take(limit + 1)
            .Select(h => new FieldValueHistoryResponse(h.Id, h.OldValue, h.NewValue, h.ChangedAtUtc, h.ChangedBy))
            .ToListAsync(cancellationToken);

        if (rows.Count <= limit)
        {
            return new FieldValueHistoryPage(rows, NextAfter: null);
        }

        List<FieldValueHistoryResponse> items = rows[..limit];

        return new FieldValueHistoryPage(items, items[^1].Id);
    }
}
