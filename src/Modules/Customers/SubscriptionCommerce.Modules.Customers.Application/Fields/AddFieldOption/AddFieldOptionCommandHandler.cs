using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.AddFieldOption;

internal sealed class AddFieldOptionCommandHandler(
    ICustomFieldDefinitionRepository fieldRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddFieldOptionCommand, int>
{
    public async Task<Result<int>> Handle(AddFieldOptionCommand command, CancellationToken cancellationToken)
    {
        CustomFieldDefinition? field = await fieldRepository.GetAsync(command.FieldId, cancellationToken);

        if (field is null)
        {
            return Result.Failure<int>(CustomFieldErrors.NotFound(command.FieldId));
        }

        Result<CustomFieldOption> option = field.AddOption(command.Label, command.SortOrder);

        if (option.IsFailure)
        {
            return Result.Failure<int>(option.Error);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // Another request added the same label between loading the field and this save.
            return Result.Failure<int>(CustomFieldErrors.DuplicateOptionLabel);
        }

        return option.Value.Id;
    }
}
