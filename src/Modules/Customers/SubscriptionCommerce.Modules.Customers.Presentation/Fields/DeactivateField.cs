using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.Fields.DeactivateField;

namespace SubscriptionCommerce.Modules.Customers.Presentation.Fields;

// A soft delete, so PUT .../deactivate rather than DELETE.
internal sealed class DeactivateField : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("fields/{fieldId:int}/deactivate", async (
            int fieldId,
            ICommandHandler<DeactivateFieldCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new DeactivateFieldCommand(fieldId), cancellationToken);

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .WithTags(Tags.CustomFields);
    }
}
