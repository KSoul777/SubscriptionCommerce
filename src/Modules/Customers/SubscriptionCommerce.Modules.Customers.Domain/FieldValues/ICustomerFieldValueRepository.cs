namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

public interface ICustomerFieldValueRepository
{
    // Includes the selected options, not the history.
    Task<CustomerFieldValue?> GetAsync(int customerId, int fieldDefinitionId, CancellationToken cancellationToken = default);

    // Read-only, with selected options; for building a customer profile.
    Task<IReadOnlyList<CustomerFieldValue>> GetForCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    // Oldest first, ordered by the identity column.
    Task<IReadOnlyList<CustomerFieldValueHistory>> GetHistoryAsync(
        int customerId,
        int fieldDefinitionId,
        CancellationToken cancellationToken = default);

    void Insert(CustomerFieldValue value);
}
