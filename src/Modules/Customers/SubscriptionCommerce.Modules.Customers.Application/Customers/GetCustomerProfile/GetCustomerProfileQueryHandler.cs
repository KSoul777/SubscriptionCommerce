using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;

namespace SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;

internal sealed class GetCustomerProfileQueryHandler(ICustomerProfileReader reader)
    : IQueryHandler<GetCustomerProfileQuery, CustomerProfileResponse>
{
    public async Task<Result<CustomerProfileResponse>> Handle(
        GetCustomerProfileQuery query,
        CancellationToken cancellationToken)
    {
        CustomerProfileResponse? profile = await reader.GetAsync(query.TenantId, query.CustomerId, cancellationToken);

        return profile is null
            ? Result.Failure<CustomerProfileResponse>(CustomerErrors.NotFound(query.CustomerId))
            : Result.Success(profile);
    }
}
