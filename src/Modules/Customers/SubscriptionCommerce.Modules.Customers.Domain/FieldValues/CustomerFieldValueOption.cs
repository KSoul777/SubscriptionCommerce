namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

// One selected option of a dropdown value.
public sealed class CustomerFieldValueOption
{
    private CustomerFieldValueOption()
    {
    }

    public int CustomerFieldValueId { get; private set; }

    public int OptionId { get; private set; }

    internal static CustomerFieldValueOption Create(int optionId) => new() { OptionId = optionId };
}
