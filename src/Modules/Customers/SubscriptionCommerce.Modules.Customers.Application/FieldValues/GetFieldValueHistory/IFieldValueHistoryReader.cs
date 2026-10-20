namespace SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;

public interface IFieldValueHistoryReader
{
    Task<FieldValueHistoryPage?> GetPageAsync(
        int tenantId,
        int customerId,
        int fieldId,
        long? after,
        int limit,
        CancellationToken cancellationToken);
}
