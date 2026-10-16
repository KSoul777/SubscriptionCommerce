using SubscriptionCommerce.Common.Application.Authentication;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.CreateField;

internal sealed class CreateFieldCommandHandler(
    ITenantRepository tenantRepository,
    ICustomFieldDefinitionRepository fieldRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ICommandHandler<CreateFieldCommand, int>
{
    public async Task<Result<int>> Handle(CreateFieldCommand command, CancellationToken cancellationToken)
    {
        if (!await tenantRepository.ExistsAsync(command.TenantId, cancellationToken))
        {
            return Result.Failure<int>(TenantErrors.NotFound(command.TenantId));
        }

        if (await fieldRepository.NameExistsAsync(command.TenantId, command.Name.Trim(), cancellationToken))
        {
            return Result.Failure<int>(CustomFieldErrors.DuplicateName);
        }

        Result<CustomFieldDefinition> field = CustomFieldDefinition.Create(
            command.TenantId,
            command.Name,
            command.Type,
            command.AllowMultipleSelection,
            command.IsRequired,
            command.SortOrder,
            currentUser.UserName,
            timeProvider.GetUtcNow().UtcDateTime);

        if (field.IsFailure)
        {
            return Result.Failure<int>(field.Error);
        }

        fieldRepository.Insert(field.Value);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // Another request created the same name between the check above and this save.
            return Result.Failure<int>(CustomFieldErrors.DuplicateName);
        }

        return field.Value.Id;
    }
}
