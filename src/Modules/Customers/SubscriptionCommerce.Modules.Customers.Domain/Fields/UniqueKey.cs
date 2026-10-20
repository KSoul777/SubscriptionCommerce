namespace SubscriptionCommerce.Modules.Customers.Domain.Fields;

public static class UniqueKey
{
    public static string For(string name) => name.Trim().ToUpperInvariant();
}
