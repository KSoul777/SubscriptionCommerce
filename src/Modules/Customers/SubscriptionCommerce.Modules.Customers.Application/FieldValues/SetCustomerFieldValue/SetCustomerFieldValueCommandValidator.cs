using FluentValidation;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;

internal sealed class SetCustomerFieldValueCommandValidator : AbstractValidator<SetCustomerFieldValueCommand>
{
    public SetCustomerFieldValueCommandValidator()
    {
        RuleFor(c => c.CustomerId).GreaterThan(0);
        RuleFor(c => c.FieldId).GreaterThan(0);
        RuleFor(c => c.TextValue).MaximumLength(CustomerFieldValue.TextMaxLength);
        RuleFor(c => c.SelectedOptionIds).NotNull();
        RuleFor(c => c.RowVersion)
            .Must(BeARowVersion)
            .When(c => !string.IsNullOrEmpty(c.RowVersion))
            .WithMessage("The row version must be the base64 value returned by the API");
    }

    // A SQL Server rowversion is 8 bytes.
    private static bool BeARowVersion(string? rowVersion)
    {
        Span<byte> bytes = stackalloc byte[8];

        return Convert.TryFromBase64String(rowVersion!, bytes, out int written) && written == 8;
    }
}
