namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

public sealed class TestTimeProvider : TimeProvider
{
    public DateTimeOffset? FrozenAt { get; set; }

    public override DateTimeOffset GetUtcNow() => FrozenAt ?? base.GetUtcNow();
}
