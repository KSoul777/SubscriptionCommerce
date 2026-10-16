using Microsoft.EntityFrameworkCore;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Customers;

internal sealed class CustomerRepository(CustomersDbContext context) : ICustomerRepository
{
    public Task<Customer?> GetAsync(int customerId, CancellationToken cancellationToken = default) =>
        context.Customers.SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

    public void Insert(Customer customer)
    {
        context.Customers.Add(customer);
    }
}
