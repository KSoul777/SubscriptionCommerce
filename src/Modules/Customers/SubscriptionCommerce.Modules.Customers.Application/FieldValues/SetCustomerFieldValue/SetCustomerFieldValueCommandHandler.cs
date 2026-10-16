using SubscriptionCommerce.Common.Application.Authentication;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

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
        Customer? customer = await customerRepository.GetAsync(command.CustomerId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<SetCustomerFieldValueResponse>(CustomerErrors.NotFound(command.CustomerId));
        }

        CustomFieldDefinition? field = await fieldRepository.GetAsync(command.FieldId, cancellationToken);

        // A field of another tenant is reported as not found, so tenants can't probe each other's fields.
        if (field is null || field.TenantId != customer.TenantId)
        {
            return Result.Failure<SetCustomerFieldValueResponse>(CustomFieldErrors.NotFound(command.FieldId));
        }

        CustomerFieldValue? existing = await valueRepository.GetAsync(customer.Id, field.Id, cancellationToken);

        string? currentRowVersion = existing is null ? null : Convert.ToBase64String(existing.RowVersion);
        string? expectedRowVersion = string.IsNullOrEmpty(command.RowVersion) ? null : command.RowVersion;

        // The caller must have seen the current value. This is what keeps history transitions consecutive.
        if (!string.Equals(expectedRowVersion, currentRowVersion, StringComparison.Ordinal))
        {
            return Result.Failure<SetCustomerFieldValueResponse>(CustomerFieldValueErrors.ConcurrencyConflict);
        }

        CustomerFieldValue value = existing ?? CustomerFieldValue.Create(customer.Id, field);

        Result<bool> changed = value.Change(
            field,
            command.TextValue,
            command.SelectedOptionIds,
            currentUser.UserName,
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
            // One SaveChanges: the value and its history entry commit together or not at all.
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // Someone saved between our read and our write: a stale row version, or a concurrent first value.
            return Result.Failure<SetCustomerFieldValueResponse>(CustomerFieldValueErrors.ConcurrencyConflict);
        }

        return new SetCustomerFieldValueResponse(Changed: true, Convert.ToBase64String(value.RowVersion));
    }
}
