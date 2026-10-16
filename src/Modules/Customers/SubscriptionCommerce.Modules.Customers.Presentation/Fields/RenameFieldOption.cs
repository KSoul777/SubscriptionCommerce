using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.Fields.RenameFieldOption;

namespace SubscriptionCommerce.Modules.Customers.Presentation.Fields;

internal sealed class RenameFieldOption : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("fields/{fieldId:int}/options/{optionId:int}", async (
            int fieldId,
            int optionId,
            Request request,
            ICommandHandler<RenameFieldOptionCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(
                new RenameFieldOptionCommand(fieldId, optionId, request.Label),
                cancellationToken);

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .WithTags(Tags.CustomFields);
    }

    internal sealed class Request
    {
        public string Label { get; init; } = string.Empty;
    }
}
