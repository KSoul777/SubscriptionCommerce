using FluentValidation;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.RenameFieldOption;

internal sealed class RenameFieldOptionCommandValidator : AbstractValidator<RenameFieldOptionCommand>
{
    public RenameFieldOptionCommandValidator()
    {
        RuleFor(c => c.TenantId).GreaterThan(0);
        RuleFor(c => c.FieldId).GreaterThan(0);
        RuleFor(c => c.OptionId).GreaterThan(0);
    }
}
