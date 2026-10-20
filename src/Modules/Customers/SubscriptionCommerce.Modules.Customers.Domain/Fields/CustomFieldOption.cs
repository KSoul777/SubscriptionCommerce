namespace SubscriptionCommerce.Modules.Customers.Domain.Fields;

public sealed class CustomFieldOption
{
    public const int LabelMaxLength = 200;

    private CustomFieldOption()
    {
    }

    public int Id { get; private set; }

    public int FieldDefinitionId { get; private set; }

    public string Label { get; private set; } = null!;

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    internal static CustomFieldOption Create(string label, int sortOrder) =>
        new()
        {
            Label = label,
            SortOrder = sortOrder,
            IsActive = true
        };

    internal void Rename(string label)
    {
        Label = label;
    }

    internal void Deactivate()
    {
        IsActive = false;
    }
}
