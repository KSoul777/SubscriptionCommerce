using FluentValidation;

namespace SubscriptionCommerce.Modules.Customers.Application.Customers.CreateCustomer;

internal sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.TenantId).GreaterThan(0);
    }
}
