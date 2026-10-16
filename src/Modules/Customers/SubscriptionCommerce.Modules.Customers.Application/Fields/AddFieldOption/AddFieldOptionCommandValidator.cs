using FluentValidation;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.AddFieldOption;

internal sealed class AddFieldOptionCommandValidator : AbstractValidator<AddFieldOptionCommand>
{
    public AddFieldOptionCommandValidator()
    {
        RuleFor(c => c.FieldId).GreaterThan(0);
        RuleFor(c => c.Label).NotEmpty().MaximumLength(CustomFieldOption.LabelMaxLength);
    }
}
