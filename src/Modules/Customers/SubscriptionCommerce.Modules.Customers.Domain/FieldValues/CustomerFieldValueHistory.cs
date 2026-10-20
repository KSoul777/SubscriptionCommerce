namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

public sealed class CustomerFieldValueHistory
{
    private CustomerFieldValueHistory()
    {
    }

    public long Id { get; private set; }

    public int CustomerFieldValueId { get; private set; }

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
