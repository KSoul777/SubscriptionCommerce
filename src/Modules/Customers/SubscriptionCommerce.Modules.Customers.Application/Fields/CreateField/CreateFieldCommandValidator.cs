using FluentValidation;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.CreateField;

internal sealed class CreateFieldCommandValidator : AbstractValidator<CreateFieldCommand>
{
    public CreateFieldCommandValidator()
    {
        RuleFor(c => c.TenantId).GreaterThan(0);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(CustomFieldDefinition.NameMaxLength);
        RuleFor(c => c.Type).IsInEnum();
    }
}
