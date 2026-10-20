using System.Collections.Immutable;

namespace SubscriptionCommerce.Modules.Customers.Domain.Fields;

public sealed record FieldValueRules
{
    internal FieldValueRules(
        int fieldDefinitionId,
        FieldType type,
        bool isRequired,
        bool allowMultipleSelection,
        bool isActive,
        ImmutableArray<FieldOptionRule> options)
    {
        FieldDefinitionId = fieldDefinitionId;
        Type = type;
        IsRequired = isRequired;
        AllowMultipleSelection = allowMultipleSelection;
        IsActive = isActive;
        Options = options;
    }

    public int FieldDefinitionId { get; }

    public FieldType Type { get; }

    public bool IsRequired { get; }

    public bool AllowMultipleSelection { get; }

    public bool IsActive { get; }

    public ImmutableArray<FieldOptionRule> Options { get; }
}
