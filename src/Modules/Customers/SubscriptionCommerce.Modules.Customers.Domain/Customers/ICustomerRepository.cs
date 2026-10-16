namespace SubscriptionCommerce.Modules.Customers.Domain.Customers;

public interface ICustomerRepository
{
    Task<Customer?> GetAsync(int customerId, CancellationToken cancellationToken = default);

    void Insert(Customer customer);
}
