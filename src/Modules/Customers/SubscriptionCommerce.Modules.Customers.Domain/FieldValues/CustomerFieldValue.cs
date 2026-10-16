using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

// The value of one custom field for one customer, and its history.
// History is part of this aggregate on purpose: a change and its history entry are saved together,
// in one SaveChanges, so there is never a value without its transition or a transition without its value.
public sealed class CustomerFieldValue
{
    public const int TextMaxLength = 2000;

    private readonly List<CustomerFieldValueOption> _selectedOptions = [];
    private readonly List<CustomerFieldValueHistory> _history = [];

    private CustomerFieldValue()
    {
    }

    public int Id { get; private set; }

    public int CustomerId { get; private set; }

    public int FieldDefinitionId { get; private set; }

    // Text fields only; null for dropdowns.
    public string? TextValue { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public string UpdatedBy { get; private set; } = null!;

    // SQL Server rowversion. A stale value makes the save fail, which keeps history transitions consecutive.
    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<CustomerFieldValueOption> SelectedOptions => _selectedOptions;

    // Only the entries added since the value was loaded: history is never loaded with the value.
    public IReadOnlyCollection<CustomerFieldValueHistory> History => _history;

    // Not saved yet: the database assigns the Id.
    private bool IsNew => Id == 0;

    // An empty value that is not saved until the first change.
    public static CustomerFieldValue Create(int customerId, CustomFieldDefinition field) =>
        new()
        {
            CustomerId = customerId,
            FieldDefinitionId = field.Id
        };

    // Returns false, and changes nothing, when the requested value equals the current one.
    public Result<bool> Change(
        CustomFieldDefinition field,
        string? text,
        IReadOnlyCollection<int> optionIds,
        string changedBy,
        DateTime utcNow)
    {
        if (field.Id != FieldDefinitionId)
        {
            throw new ArgumentException("The field definition does not belong to this value", nameof(field));
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

        // The option must belong to the field. A newly picked option must also be active, but one that
        // stays selected may have been deactivated since it was picked.
        bool CanBeSelected(int optionId) =>
            fieldOptions.TryGetValue(optionId, out CustomFieldOption? option) &&
            (option.IsActive || currentOptionIds.Contains(optionId));

        if (!newOptionIds.All(CanBeSelected))
        {
            return Result.Failure<bool>(CustomerFieldValueErrors.InvalidOption);
        }

        FieldValueSnapshot? oldValue = IsNew ? null : Snapshot(TextValue, currentOptionIds, fieldOptions);
        FieldValueSnapshot newValue = Snapshot(newText, newOptionIds, fieldOptions);

        TextValue = newText;
        _selectedOptions.RemoveAll(o => !newOptionIds.Contains(o.OptionId));
        _selectedOptions.AddRange(newOptionIds
            .Where(id => !currentOptionIds.Contains(id))
            .Select(CustomerFieldValueOption.Create));
        UpdatedAtUtc = utcNow;
        UpdatedBy = changedBy;

        _history.Add(CustomerFieldValueHistory.Create(oldValue, newValue, changedBy, utcNow));

        return true;
    }

    private static Result ValidateShape(CustomFieldDefinition field, string? text, int[] optionIds)
    {
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
        Dictionary<int, CustomFieldOption> fieldOptions) =>
        new(
            text,
            optionIds
                .Select(id => fieldOptions[id])
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.Id)
                .Select(o => new SnapshotOption(o.Id, o.Label))
                .ToList());
}
