using FluentValidation;

namespace SubscriptionCommerce.Modules.Customers.Application.Fields.CreateField;

internal sealed class CreateFieldCommandValidator : AbstractValidator<CreateFieldCommand>
{
    public CreateFieldCommandValidator()
    {
        RuleFor(c => c.TenantId).GreaterThan(0);
    }
}
