using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

public static class CustomerFieldValueErrors
{
    public static readonly Error ValueRequired = Error.Problem(
        "CustomerFieldValues.ValueRequired",
        "The field is required");

    public static readonly Error TextNotAllowed = Error.Problem(
        "CustomerFieldValues.TextNotAllowed",
        "A dropdown field takes selected options, not text");

    public static readonly Error OptionsNotAllowed = Error.Problem(
        "CustomerFieldValues.OptionsNotAllowed",
        "A text field takes text, not selected options");

    public static readonly Error MultipleSelectionNotAllowed = Error.Problem(
        "CustomerFieldValues.MultipleSelectionNotAllowed",
        "The field allows only one selected option");

    public static readonly Error InvalidOption = Error.Problem(
        "CustomerFieldValues.InvalidOption",
        "Each selected option must be an active option of the field");

    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "CustomerFieldValues.ConcurrencyConflict",
        "The value was changed by someone else. Reload it and retry with its current row version");
}
