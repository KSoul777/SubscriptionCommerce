using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.RenameFieldOption;

internal sealed class RenameFieldOptionCommandHandler(
    ICustomFieldDefinitionRepository fieldRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RenameFieldOptionCommand>
{
    public async Task<Result> Handle(RenameFieldOptionCommand command, CancellationToken cancellationToken)
    {
        CustomFieldDefinition? field = await fieldRepository.GetAsync(command.TenantId, command.FieldId, cancellationToken);

        if (field is null)
        {
            return Result.Failure(CustomFieldErrors.NotFound(command.FieldId));
        }

        Result renamed = field.RenameOption(command.OptionId, command.Label);

        if (renamed.IsFailure)
        {
            return renamed;
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException exception) when (exception.Kind == ConflictKind.DuplicateOptionLabel)
        {
            return Result.Failure(CustomFieldErrors.DuplicateOptionLabel);
        }

        return Result.Success();
    }
}
