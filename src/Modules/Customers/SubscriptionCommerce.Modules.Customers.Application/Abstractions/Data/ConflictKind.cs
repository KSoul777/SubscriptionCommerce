namespace SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;

public enum ConflictKind
{
    RowVersion,
    DuplicateFieldName,
    DuplicateOptionLabel,
    DuplicateFieldValue,
    DuplicateSelectedOption
}
