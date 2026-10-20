using SubscriptionCommerce.Common.Application.Authentication;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;
using SubscriptionCommerce.Modules.Customers.Domain.Users;

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
        Result<UserName> createdBy = UserName.Create(currentUser.UserName);

        if (createdBy.IsFailure)
        {
            return Result.Failure<int>(createdBy.Error);
        }

        if (!await tenantRepository.ExistsAsync(command.TenantId, cancellationToken))
        {
            return Result.Failure<int>(TenantErrors.NotFound(command.TenantId));
        }

        Result<CustomFieldDefinition> field = CustomFieldDefinition.Create(
            command.TenantId,
            command.Name,
            command.Type,
            command.AllowMultipleSelection,
            command.IsRequired,
            command.SortOrder,
            createdBy.Value,
            timeProvider.GetUtcNow().UtcDateTime);

        if (field.IsFailure)
        {
            return Result.Failure<int>(field.Error);
        }

        if (await fieldRepository.NameExistsAsync(command.TenantId, field.Value.Name, cancellationToken))
        {
            return Result.Failure<int>(CustomFieldErrors.DuplicateName);
        }

        fieldRepository.Insert(field.Value);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException exception) when (exception.Kind == ConflictKind.DuplicateFieldName)
        {
            return Result.Failure<int>(CustomFieldErrors.DuplicateName);
        }

        return field.Value.Id;
    }
}
