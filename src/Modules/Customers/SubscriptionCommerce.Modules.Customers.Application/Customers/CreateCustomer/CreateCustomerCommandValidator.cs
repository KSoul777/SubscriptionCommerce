using FluentValidation;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;

namespace SubscriptionCommerce.Modules.Customers.Application.Customers.CreateCustomer;

internal sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.TenantId).GreaterThan(0);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Customer.NameMaxLength);
    }
}
