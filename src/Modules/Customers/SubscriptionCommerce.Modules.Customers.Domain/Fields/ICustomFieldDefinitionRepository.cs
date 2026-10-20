namespace SubscriptionCommerce.Modules.Customers.Domain.Fields;

public interface ICustomFieldDefinitionRepository
{
    Task<CustomFieldDefinition?> GetAsync(int tenantId, int fieldId, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(int tenantId, string name, CancellationToken cancellationToken = default);

    void Insert(CustomFieldDefinition field);
}
