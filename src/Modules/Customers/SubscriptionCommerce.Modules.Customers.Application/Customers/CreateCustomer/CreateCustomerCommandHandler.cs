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

        Result<Customer> customer = Customer.Create(command.TenantId, command.Name, timeProvider.GetUtcNow().UtcDateTime);

        if (customer.IsFailure)
        {
            return Result.Failure<int>(customer.Error);
        }

        customerRepository.Insert(customer.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.Value.Id;
    }
}
