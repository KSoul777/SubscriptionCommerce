namespace SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;

public interface ICustomerProfileReader
{
    Task<CustomerProfileResponse?> GetAsync(int tenantId, int customerId, CancellationToken cancellationToken);
}
