namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

// A value as it was at one moment. Option labels are copied in, so renaming an option later
// does not rewrite history.
public sealed record FieldValueSnapshot(string? Text, IReadOnlyList<SnapshotOption> Options);

public sealed record SnapshotOption(int Id, string Label);
