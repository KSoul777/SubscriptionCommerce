using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Customers.Domain.Customers;

public sealed class Customer
{
    public const int NameMaxLength = 300;

    private Customer()
    {
    }

    public int Id { get; private set; }

    public int TenantId { get; private set; }

    public string Name { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public static Result<Customer> Create(int tenantId, string? name, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > NameMaxLength)
        {
            return Result.Failure<Customer>(CustomerErrors.NameInvalid);
        }

        return new Customer
        {
            TenantId = tenantId,
            Name = name.Trim(),
            CreatedAtUtc = utcNow
        };
    }
}
