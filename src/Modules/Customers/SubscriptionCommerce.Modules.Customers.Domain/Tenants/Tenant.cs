using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Customers.Domain.Tenants;

public sealed class Tenant
{
    public const int NameMaxLength = 200;

    private Tenant()
    {
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public static Result<Tenant> Create(string? name, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > NameMaxLength)
        {
            return Result.Failure<Tenant>(TenantErrors.NameInvalid);
        }

        return new Tenant
        {
            Name = name.Trim(),
            CreatedAtUtc = utcNow
        };
    }
}
