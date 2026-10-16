namespace SubscriptionCommerce.Modules.Customers.Domain.Tenants;

// A company using the ERP. Custom fields are defined per tenant.
public sealed class Tenant
{
    public const int NameMaxLength = 200;

    private Tenant()
    {
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public static Tenant Create(string name, DateTime utcNow) =>
        new()
        {
            Name = name.Trim(),
            CreatedAtUtc = utcNow
        };
}
