namespace SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;

public interface IUnitOfWork
{
    // Throws ConcurrencyConflictException when another request wrote first.
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
