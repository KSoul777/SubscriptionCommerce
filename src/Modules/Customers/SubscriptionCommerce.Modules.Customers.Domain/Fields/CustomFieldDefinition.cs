using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Users;

namespace SubscriptionCommerce.Modules.Customers.Domain.Fields;

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

    public bool AllowMultipleSelection { get; private set; }

    public bool IsRequired { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public string CreatedBy { get; private set; } = null!;

    public IReadOnlyCollection<CustomFieldOption> Options => _options.AsReadOnly();

    public static Result<CustomFieldDefinition> Create(
        int tenantId,
        string? name,
        FieldType type,
        bool allowMultipleSelection,
        bool isRequired,
        int sortOrder,
        UserName createdBy,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > NameMaxLength)
        {
            return Result.Failure<CustomFieldDefinition>(CustomFieldErrors.NameInvalid);
        }

        if (!Enum.IsDefined(type))
        {
            return Result.Failure<CustomFieldDefinition>(CustomFieldErrors.UnknownType);
        }

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
            CreatedBy = createdBy.Value
        };
    }

    public Result<CustomFieldOption> AddOption(string? label, int sortOrder)
    {
        if (!IsActive)
        {
            return Result.Failure<CustomFieldOption>(CustomFieldErrors.Inactive);
        }

        if (Type != FieldType.Dropdown)
        {
            return Result.Failure<CustomFieldOption>(CustomFieldErrors.OptionsRequireDropdown);
        }

        Result<string> validLabel = ValidateLabel(label, optionId: null);

        if (validLabel.IsFailure)
        {
            return Result.Failure<CustomFieldOption>(validLabel.Error);
        }

        var option = CustomFieldOption.Create(validLabel.Value, sortOrder);

        _options.Add(option);

        return option;
    }

    public Result RenameOption(int optionId, string? label)
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

        Result<string> validLabel = ValidateLabel(label, optionId);

        if (validLabel.IsFailure)
        {
            return validLabel;
        }

        option.Rename(validLabel.Value);

        return Result.Success();
    }

    public Result DeactivateOption(int optionId)
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

        option.Deactivate();

        return Result.Success();
    }

    public FieldValueRules ValueRules() =>
        new(
            Id,
            Type,
            IsRequired,
            AllowMultipleSelection,
            IsActive,
            [.. _options.Select(o => new FieldOptionRule(o.Id, o.Label, o.SortOrder, o.IsActive))]);

    public void Deactivate()
    {
        IsActive = false;
    }

    private Result<string> ValidateLabel(string? label, int? optionId)
    {
        if (string.IsNullOrWhiteSpace(label) || label.Trim().Length > CustomFieldOption.LabelMaxLength)
        {
            return Result.Failure<string>(CustomFieldErrors.OptionLabelInvalid);
        }

        string trimmed = label.Trim();
        string key = UniqueKey.For(trimmed);

        if (_options.Exists(o => o.Id != optionId && string.Equals(UniqueKey.For(o.Label), key, StringComparison.Ordinal)))
        {
            return Result.Failure<string>(CustomFieldErrors.DuplicateOptionLabel);
        }

        return trimmed;
    }
}
