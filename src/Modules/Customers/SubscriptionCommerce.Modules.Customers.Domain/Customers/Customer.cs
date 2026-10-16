namespace SubscriptionCommerce.Modules.Customers.Domain.Customers;

// Stands in for the ERP's existing customer table: just enough to attach field values to.
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

    public static Customer Create(int tenantId, string name, DateTime utcNow) =>
        new()
        {
            TenantId = tenantId,
            Name = name.Trim(),
            CreatedAtUtc = utcNow
        };
}
