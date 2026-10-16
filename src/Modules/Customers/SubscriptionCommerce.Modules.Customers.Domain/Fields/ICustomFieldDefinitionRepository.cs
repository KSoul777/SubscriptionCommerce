namespace SubscriptionCommerce.Modules.Customers.Domain.Fields;

public interface ICustomFieldDefinitionRepository
{
    // Includes all options, active and inactive.
    Task<CustomFieldDefinition?> GetAsync(int fieldId, CancellationToken cancellationToken = default);

    // Read-only, with options; for building a customer profile.
    Task<IReadOnlyList<CustomFieldDefinition>> GetActiveForTenantAsync(int tenantId, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(int tenantId, string name, CancellationToken cancellationToken = default);

    void Insert(CustomFieldDefinition field);
}
