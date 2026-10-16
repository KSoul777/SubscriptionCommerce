using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Customers.Domain.Fields;

// A field the tenant's users add to every customer profile. The type is fixed at creation:
// there is no way to change it, so existing values can never end up with the wrong shape.
public sealed class CustomFieldDefinition
{
    public const int NameMaxLength = 200;

    private readonly List<CustomFieldOption> _options = [];

    private CustomFieldDefinition()
    {
    }

    public int Id { get; private set; }

    public int TenantId { get; private set; }

    public string Name { get; private set; } = null!;

    public FieldType Type { get; private set; }

    // Dropdown only. The brief's "dropdown with multiple selectable values" can mean either; both are supported.
    public bool AllowMultipleSelection { get; private set; }

    public bool IsRequired { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public string CreatedBy { get; private set; } = null!;

    public IReadOnlyCollection<CustomFieldOption> Options => _options;

    public static Result<CustomFieldDefinition> Create(
        int tenantId,
        string name,
        FieldType type,
        bool allowMultipleSelection,
        bool isRequired,
        int sortOrder,
        string createdBy,
        DateTime utcNow)
    {
        if (type == FieldType.Text && allowMultipleSelection)
        {
            return Result.Failure<CustomFieldDefinition>(CustomFieldErrors.MultipleSelectionRequiresDropdown);
        }

        return new CustomFieldDefinition
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Type = type,
            AllowMultipleSelection = allowMultipleSelection,
            IsRequired = isRequired,
            SortOrder = sortOrder,
            IsActive = true,
            CreatedAtUtc = utcNow,
            CreatedBy = createdBy
        };
    }

    public Result<CustomFieldOption> AddOption(string label, int sortOrder)
    {
        if (!IsActive)
        {
            return Result.Failure<CustomFieldOption>(CustomFieldErrors.Inactive);
        }

        if (Type != FieldType.Dropdown)
        {
            return Result.Failure<CustomFieldOption>(CustomFieldErrors.OptionsRequireDropdown);
        }

        string trimmed = label.Trim();

        if (_options.Exists(o => string.Equals(o.Label, trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<CustomFieldOption>(CustomFieldErrors.DuplicateOptionLabel);
        }

        var option = CustomFieldOption.Create(trimmed, sortOrder);

        _options.Add(option);

        return option;
    }

    // History keeps the label as it was when each value was set, so a rename never rewrites the past.
    public Result RenameOption(int optionId, string label)
    {
        if (!IsActive)
        {
            return Result.Failure(CustomFieldErrors.Inactive);
        }

        CustomFieldOption? option = _options.Find(o => o.Id == optionId);

        if (option is null)
        {
            return Result.Failure(CustomFieldErrors.OptionNotFound(optionId));
        }

        string trimmed = label.Trim();

        // The option itself may keep its label and only change the casing.
        if (_options.Exists(o => o.Id != optionId && string.Equals(o.Label, trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure(CustomFieldErrors.DuplicateOptionLabel);
        }

        option.Rename(trimmed);

        return Result.Success();
    }

    // Soft delete: the option stays, so existing selections and history still resolve.
    // It can no longer be newly picked. Deactivating an inactive option changes nothing.
    public Result DeactivateOption(int optionId)
    {
        CustomFieldOption? option = _options.Find(o => o.Id == optionId);

        if (option is null)
        {
            return Result.Failure(CustomFieldErrors.OptionNotFound(optionId));
        }

        option.Deactivate();

        return Result.Success();
    }

    // Soft delete: values and history are kept and history stays readable. The field leaves customer
    // profiles, and its values and options can no longer change. Deactivating twice changes nothing.
    public void Deactivate()
    {
        IsActive = false;
    }
}
