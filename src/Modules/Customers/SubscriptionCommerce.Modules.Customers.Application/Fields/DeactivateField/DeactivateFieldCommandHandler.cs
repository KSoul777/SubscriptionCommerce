using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.DeactivateField;

internal sealed class DeactivateFieldCommandHandler(
    ICustomFieldDefinitionRepository fieldRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateFieldCommand>
{
    public async Task<Result> Handle(DeactivateFieldCommand command, CancellationToken cancellationToken)
    {
        CustomFieldDefinition? field = await fieldRepository.GetAsync(command.FieldId, cancellationToken);

        if (field is null)
        {
            return Result.Failure(CustomFieldErrors.NotFound(command.FieldId));
        }

        field.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
