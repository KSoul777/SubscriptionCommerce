using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.Users;

namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

public sealed class CustomerFieldValue
{
    public const int TextMaxLength = 2000;

    private readonly List<CustomerFieldValueOption> _selectedOptions = [];
    private readonly List<CustomerFieldValueHistory> _pendingHistory = [];

    private bool _isNew;

    private CustomerFieldValue()
    {
    }

    public int Id { get; private set; }

    public int TenantId { get; private set; }

    public int CustomerId { get; private set; }

    public int FieldDefinitionId { get; private set; }

    public string? TextValue { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public string UpdatedBy { get; private set; } = null!;

    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<CustomerFieldValueOption> SelectedOptions => _selectedOptions.AsReadOnly();

    public IReadOnlyCollection<CustomerFieldValueHistory> PendingHistory => _pendingHistory.AsReadOnly();

    public static CustomerFieldValue Create(Customer customer, CustomFieldDefinition field)
    {
        if (customer.TenantId != field.TenantId)
        {
            throw new ArgumentException("The customer and the field belong to different tenants", nameof(field));
        }

        return new CustomerFieldValue
        {
            TenantId = customer.TenantId,
            CustomerId = customer.Id,
            FieldDefinitionId = field.Id,
            _isNew = true
        };
    }

    public Result<bool> Change(
        FieldValueRules field,
        string? text,
        IReadOnlyCollection<int> optionIds,
        UserName changedBy,
        DateTime utcNow)
    {
        if (field.FieldDefinitionId != FieldDefinitionId)
        {
            throw new ArgumentException("The field rules do not belong to this value", nameof(field));
        }

        if (!field.IsActive)
        {
            return Result.Failure<bool>(CustomFieldErrors.Inactive);
        }

        string? newText = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        int[] newOptionIds = optionIds.Distinct().ToArray();

        Result shape = ValidateShape(field, newText, newOptionIds);

        if (shape.IsFailure)
        {
            return Result.Failure<bool>(shape.Error);
        }

        var currentOptionIds = _selectedOptions.Select(o => o.OptionId).ToHashSet();

        if (newText == TextValue && currentOptionIds.SetEquals(newOptionIds))
        {
            return false;
        }

        var fieldOptions = field.Options.ToDictionary(o => o.Id);

        bool CanBeSelected(int optionId) =>
            fieldOptions.TryGetValue(optionId, out FieldOptionRule? option) &&
            (option.IsActive || currentOptionIds.Contains(optionId));

        if (!newOptionIds.All(CanBeSelected))
        {
            return Result.Failure<bool>(CustomerFieldValueErrors.InvalidOption);
        }

        FieldValueSnapshot? oldValue = _isNew ? null : Snapshot(TextValue, currentOptionIds, fieldOptions);
        FieldValueSnapshot newValue = Snapshot(newText, newOptionIds, fieldOptions);

        TextValue = newText;
        _selectedOptions.RemoveAll(o => !newOptionIds.Contains(o.OptionId));
        _selectedOptions.AddRange(newOptionIds
            .Where(id => !currentOptionIds.Contains(id))
            .Select(optionId => CustomerFieldValueOption.Create(FieldDefinitionId, optionId)));
        UpdatedAtUtc = utcNow;
        UpdatedBy = changedBy.Value;
        _isNew = false;

        _pendingHistory.Add(CustomerFieldValueHistory.Create(oldValue, newValue, changedBy.Value, utcNow));

        return true;
    }

    private static Result ValidateShape(FieldValueRules field, string? text, int[] optionIds)
    {
        if (text?.Length > TextMaxLength)
        {
            return Result.Failure(CustomerFieldValueErrors.TextTooLong);
        }

        if (field.Type == FieldType.Text && optionIds.Length > 0)
        {
            return Result.Failure(CustomerFieldValueErrors.OptionsNotAllowed);
        }

        if (field.Type == FieldType.Dropdown && text is not null)
        {
            return Result.Failure(CustomerFieldValueErrors.TextNotAllowed);
        }

        if (field.Type == FieldType.Dropdown && !field.AllowMultipleSelection && optionIds.Length > 1)
        {
            return Result.Failure(CustomerFieldValueErrors.MultipleSelectionNotAllowed);
        }

        if (field.IsRequired && text is null && optionIds.Length == 0)
        {
            return Result.Failure(CustomerFieldValueErrors.ValueRequired);
        }

        return Result.Success();
    }

    private static FieldValueSnapshot Snapshot(
        string? text,
        IEnumerable<int> optionIds,
        Dictionary<int, FieldOptionRule> fieldOptions) =>
        new(
            text,
            optionIds
                .Select(id => fieldOptions[id])
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.Id)
                .Select(o => new SnapshotOption(o.Id, o.Label))
                .ToList()
                .AsReadOnly());
}
