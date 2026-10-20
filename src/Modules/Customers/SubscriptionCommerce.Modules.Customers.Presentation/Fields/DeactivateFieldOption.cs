using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.Fields.DeactivateFieldOption;

namespace SubscriptionCommerce.Modules.Customers.Presentation.Fields;

internal sealed class DeactivateFieldOption : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("tenants/{tenantId:int}/fields/{fieldId:int}/options/{optionId:int}/deactivate", async (
            int tenantId,
            int fieldId,
            int optionId,
            ICommandHandler<DeactivateFieldOptionCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new DeactivateFieldOptionCommand(tenantId, fieldId, optionId), cancellationToken);

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .WithTags(Tags.CustomFields);
    }
}
