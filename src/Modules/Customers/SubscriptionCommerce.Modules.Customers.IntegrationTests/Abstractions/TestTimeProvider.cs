namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

// Real time unless a test freezes it. The integration tests share one collection, so they run
// one at a time and a frozen clock can't leak into another test that is running.
public sealed class TestTimeProvider : TimeProvider
{
    public DateTimeOffset? FrozenAt { get; set; }

    public override DateTimeOffset GetUtcNow() => FrozenAt ?? base.GetUtcNow();
}
