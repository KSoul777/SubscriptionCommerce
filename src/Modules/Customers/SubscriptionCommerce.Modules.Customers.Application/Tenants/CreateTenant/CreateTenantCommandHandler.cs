using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;

namespace SubscriptionCommerce.Modules.Customers.Application.Tenants.CreateTenant;

internal sealed class CreateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateTenantCommand, int>
{
    public async Task<Result<int>> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        Result<Tenant> tenant = Tenant.Create(command.Name, timeProvider.GetUtcNow().UtcDateTime);

        if (tenant.IsFailure)
        {
            return Result.Failure<int>(tenant.Error);
        }

        tenantRepository.Insert(tenant.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tenant.Value.Id;
    }
}
