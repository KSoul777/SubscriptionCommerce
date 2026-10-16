using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Customers.Domain.Fields;

public static class CustomFieldErrors
{
    public static Error NotFound(int fieldId) =>
        Error.NotFound("CustomFields.NotFound", $"The custom field with the identifier {fieldId} was not found");

    public static Error OptionNotFound(int optionId) =>
        Error.NotFound("CustomFields.OptionNotFound", $"The field has no option with the identifier {optionId}");

    public static readonly Error DuplicateName = Error.Conflict(
        "CustomFields.DuplicateName",
        "A custom field with this name already exists for the tenant");

    public static readonly Error MultipleSelectionRequiresDropdown = Error.Problem(
        "CustomFields.MultipleSelectionRequiresDropdown",
        "Only dropdown fields can allow multiple selection");

    public static readonly Error OptionsRequireDropdown = Error.Problem(
        "CustomFields.OptionsRequireDropdown",
        "Only dropdown fields have options");

    public static readonly Error DuplicateOptionLabel = Error.Conflict(
        "CustomFields.DuplicateOptionLabel",
        "An option with this label already exists for the field");

    public static readonly Error Inactive = Error.Problem(
        "CustomFields.Inactive",
        "The custom field is inactive");
}
