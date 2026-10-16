using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;

namespace SubscriptionCommerce.Modules.Customers.Application.Customers.CreateCustomer;

internal sealed class CreateCustomerCommandHandler(
    ITenantRepository tenantRepository,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateCustomerCommand, int>
{
    public async Task<Result<int>> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        if (!await tenantRepository.ExistsAsync(command.TenantId, cancellationToken))
        {
            return Result.Failure<int>(TenantErrors.NotFound(command.TenantId));
        }

        var customer = Customer.Create(command.TenantId, command.Name, timeProvider.GetUtcNow().UtcDateTime);

        customerRepository.Insert(customer);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
