using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;

namespace SubscriptionCommerce.Modules.Customers.Presentation.Customers;

internal sealed class GetCustomerProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenants/{tenantId:int}/customers/{customerId:int}/profile", async (
            int tenantId,
            int customerId,
            IQueryHandler<GetCustomerProfileQuery, CustomerProfileResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<CustomerProfileResponse> result =
                await handler.Handle(new GetCustomerProfileQuery(tenantId, customerId), cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Customers);
    }
}
