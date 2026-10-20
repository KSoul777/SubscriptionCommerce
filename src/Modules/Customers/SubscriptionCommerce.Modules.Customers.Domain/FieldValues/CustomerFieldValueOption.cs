namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

public sealed class CustomerFieldValueOption
{
    private CustomerFieldValueOption()
    {
    }

    public int CustomerFieldValueId { get; private set; }

    public int FieldDefinitionId { get; private set; }

    public int OptionId { get; private set; }

    internal static CustomerFieldValueOption Create(int fieldDefinitionId, int optionId) =>
        new() { FieldDefinitionId = fieldDefinitionId, OptionId = optionId };
}
