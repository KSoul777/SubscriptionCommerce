namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

// One transition from OldValue to NewValue. Id is a database identity, so it gives the order of changes
// independent of any server clock.
public sealed class CustomerFieldValueHistory
{
    private CustomerFieldValueHistory()
    {
    }

    public long Id { get; private set; }

    public int CustomerFieldValueId { get; private set; }

    // Null for the first value.
    public FieldValueSnapshot? OldValue { get; private set; }

    public FieldValueSnapshot NewValue { get; private set; } = null!;

    public DateTime ChangedAtUtc { get; private set; }

    public string ChangedBy { get; private set; } = null!;

    internal static CustomerFieldValueHistory Create(
        FieldValueSnapshot? oldValue,
        FieldValueSnapshot newValue,
        string changedBy,
        DateTime utcNow) =>
        new()
        {
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = changedBy,
            ChangedAtUtc = utcNow
        };
}
