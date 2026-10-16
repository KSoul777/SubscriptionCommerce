using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Abstractions;

internal static class Fields
{
    internal const int FieldId = 3;

    internal static readonly DateTime UtcNow = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);

    internal static CustomFieldDefinition Text(bool isRequired = false) =>
        CustomFieldDefinition.Create(1, "Tier", FieldType.Text, false, isRequired, 0, "tester", UtcNow)
            .Value
            .Set(f => f.Id, FieldId);

    internal static CustomFieldDefinition Dropdown(params string[] labels) => CreateDropdown(false, labels);

    internal static CustomFieldDefinition MultiSelect(params string[] labels) => CreateDropdown(true, labels);

    internal static CustomFieldOption Option(this CustomFieldDefinition field, string label) =>
        field.Options.Single(o => o.Label == label);

    // Options get ids 10, 11, 12, ... and sort order 0, 1, 2, ... in the order of the labels.
    private static CustomFieldDefinition CreateDropdown(bool allowMultipleSelection, string[] labels)
    {
        CustomFieldDefinition field = CustomFieldDefinition
            .Create(1, "Contact", FieldType.Dropdown, allowMultipleSelection, false, 0, "tester", UtcNow)
            .Value
            .Set(f => f.Id, FieldId);

        for (int i = 0; i < labels.Length; i++)
        {
            field.AddOption(labels[i], sortOrder: i).Value.Set(o => o.Id, 10 + i);
        }

        return field;
    }
}
