using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.Fields.DeactivateFieldOption;

namespace SubscriptionCommerce.Modules.Customers.Presentation.Fields;

// A soft delete, so PUT .../deactivate rather than DELETE.
internal sealed class DeactivateFieldOption : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("fields/{fieldId:int}/options/{optionId:int}/deactivate", async (
            int fieldId,
            int optionId,
            ICommandHandler<DeactivateFieldOptionCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new DeactivateFieldOptionCommand(fieldId, optionId), cancellationToken);

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .WithTags(Tags.CustomFields);
    }
}
