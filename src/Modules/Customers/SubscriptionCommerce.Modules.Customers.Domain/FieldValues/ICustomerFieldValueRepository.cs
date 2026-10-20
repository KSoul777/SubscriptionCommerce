namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

public interface ICustomerFieldValueRepository
{
    Task<CustomerFieldValue?> GetAsync(int customerId, int fieldDefinitionId, CancellationToken cancellationToken = default);

    void Insert(CustomerFieldValue value);
}
