using FluentValidation;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.DeactivateFieldOption;

internal sealed class DeactivateFieldOptionCommandValidator : AbstractValidator<DeactivateFieldOptionCommand>
{
    public DeactivateFieldOptionCommandValidator()
    {
        RuleFor(c => c.FieldId).GreaterThan(0);
        RuleFor(c => c.OptionId).GreaterThan(0);
    }
}
