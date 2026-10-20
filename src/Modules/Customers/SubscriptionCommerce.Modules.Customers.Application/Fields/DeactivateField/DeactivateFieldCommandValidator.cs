using FluentValidation;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.DeactivateField;

internal sealed class DeactivateFieldCommandValidator : AbstractValidator<DeactivateFieldCommand>
{
    public DeactivateFieldCommandValidator()
    {
        RuleFor(c => c.TenantId).GreaterThan(0);
        RuleFor(c => c.FieldId).GreaterThan(0);
    }
}
