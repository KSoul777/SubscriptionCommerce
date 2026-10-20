using FluentValidation;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.AddFieldOption;

internal sealed class AddFieldOptionCommandValidator : AbstractValidator<AddFieldOptionCommand>
{
    public AddFieldOptionCommandValidator()
    {
        RuleFor(c => c.TenantId).GreaterThan(0);
        RuleFor(c => c.FieldId).GreaterThan(0);
    }
}
