using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.Tenants.CreateTenant;

namespace SubscriptionCommerce.Modules.Customers.Presentation.Tenants;

internal sealed class CreateTenant : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenants", async (
            Request request,
            ICommandHandler<CreateTenantCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            Result<int> result = await handler.Handle(new CreateTenantCommand(request.Name), cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Tenants);
    }

    internal sealed class Request
    {
        public string Name { get; init; } = string.Empty;
    }
}
