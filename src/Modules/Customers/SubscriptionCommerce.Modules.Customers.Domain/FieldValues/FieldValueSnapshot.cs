namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

public sealed record FieldValueSnapshot(string? Text, IReadOnlyList<SnapshotOption> Options);

public sealed record SnapshotOption(int Id, string Label);
