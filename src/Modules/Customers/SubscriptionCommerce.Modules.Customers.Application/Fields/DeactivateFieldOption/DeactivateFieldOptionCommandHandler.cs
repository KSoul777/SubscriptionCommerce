using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.DeactivateFieldOption;

internal sealed class DeactivateFieldOptionCommandHandler(
    ICustomFieldDefinitionRepository fieldRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateFieldOptionCommand>
{
    public async Task<Result> Handle(DeactivateFieldOptionCommand command, CancellationToken cancellationToken)
    {
        CustomFieldDefinition? field = await fieldRepository.GetAsync(command.TenantId, command.FieldId, cancellationToken);

        if (field is null)
        {
            return Result.Failure(CustomFieldErrors.NotFound(command.FieldId));
        }

        Result deactivated = field.DeactivateOption(command.OptionId);

        if (deactivated.IsFailure)
        {
            return deactivated;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
