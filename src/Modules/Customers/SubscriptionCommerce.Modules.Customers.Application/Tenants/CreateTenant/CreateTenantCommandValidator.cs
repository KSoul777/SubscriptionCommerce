using FluentValidation;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;

namespace SubscriptionCommerce.Modules.Customers.Application.Tenants.CreateTenant;

internal sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Tenant.NameMaxLength);
    }
}
