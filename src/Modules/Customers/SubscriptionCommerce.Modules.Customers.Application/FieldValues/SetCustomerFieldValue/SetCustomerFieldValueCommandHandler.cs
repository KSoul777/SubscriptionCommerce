using SubscriptionCommerce.Common.Application.Authentication;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.Domain.Users;

namespace SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;

internal sealed class SetCustomerFieldValueCommandHandler(
    ICustomerRepository customerRepository,
    ICustomFieldDefinitionRepository fieldRepository,
    ICustomerFieldValueRepository valueRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ICommandHandler<SetCustomerFieldValueCommand, SetCustomerFieldValueResponse>
{
    public async Task<Result<SetCustomerFieldValueResponse>> Handle(
        SetCustomerFieldValueCommand command,
        CancellationToken cancellationToken)
    {
        Result<UserName> changedBy = UserName.Create(currentUser.UserName);

        if (changedBy.IsFailure)
        {
            return Result.Failure<SetCustomerFieldValueResponse>(changedBy.Error);
        }

        Customer? customer = await customerRepository.GetAsync(command.TenantId, command.CustomerId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<SetCustomerFieldValueResponse>(CustomerErrors.NotFound(command.CustomerId));
        }

        CustomFieldDefinition? field = await fieldRepository.GetAsync(command.TenantId, command.FieldId, cancellationToken);

        if (field is null)
        {
            return Result.Failure<SetCustomerFieldValueResponse>(CustomFieldErrors.NotFound(command.FieldId));
        }

        CustomerFieldValue? existing = await valueRepository.GetAsync(customer.Id, field.Id, cancellationToken);

        string? currentRowVersion = existing is null ? null : Convert.ToBase64String(existing.RowVersion);
        string? expectedRowVersion = string.IsNullOrEmpty(command.RowVersion) ? null : command.RowVersion;

        if (!string.Equals(expectedRowVersion, currentRowVersion, StringComparison.Ordinal))
        {
            return Result.Failure<SetCustomerFieldValueResponse>(CustomerFieldValueErrors.ConcurrencyConflict);
        }

        CustomerFieldValue value = existing ?? CustomerFieldValue.Create(customer, field);

        Result<bool> changed = value.Change(
            field.ValueRules(),
            command.TextValue,
            command.SelectedOptionIds,
            changedBy.Value,
            timeProvider.GetUtcNow().UtcDateTime);

        if (changed.IsFailure)
        {
            return Result.Failure<SetCustomerFieldValueResponse>(changed.Error);
        }

        if (!changed.Value)
        {
            return new SetCustomerFieldValueResponse(Changed: false, currentRowVersion);
        }

        if (existing is null)
        {
            valueRepository.Insert(value);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException exception)
            when (exception.Kind is ConflictKind.RowVersion or ConflictKind.DuplicateFieldValue or ConflictKind.DuplicateSelectedOption)
        {
            return Result.Failure<SetCustomerFieldValueResponse>(CustomerFieldValueErrors.ConcurrencyConflict);
        }

        return new SetCustomerFieldValueResponse(Changed: true, Convert.ToBase64String(value.RowVersion));
    }
}
