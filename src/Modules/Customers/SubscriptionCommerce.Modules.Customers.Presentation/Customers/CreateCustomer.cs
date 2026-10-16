using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.Customers.CreateCustomer;

namespace SubscriptionCommerce.Modules.Customers.Presentation.Customers;

internal sealed class CreateCustomer : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenants/{tenantId:int}/customers", async (
            int tenantId,
            Request request,
            ICommandHandler<CreateCustomerCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            Result<int> result = await handler.Handle(new CreateCustomerCommand(tenantId, request.Name), cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Customers);
    }

    internal sealed class Request
    {
        public string Name { get; init; } = string.Empty;
    }
}
