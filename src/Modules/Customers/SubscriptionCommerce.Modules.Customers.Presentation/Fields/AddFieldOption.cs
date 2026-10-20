using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.Fields.AddFieldOption;

namespace SubscriptionCommerce.Modules.Customers.Presentation.Fields;

internal sealed class AddFieldOption : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenants/{tenantId:int}/fields/{fieldId:int}/options", async (
            int tenantId,
            int fieldId,
            Request request,
            ICommandHandler<AddFieldOptionCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            Result<int> result = await handler.Handle(
                new AddFieldOptionCommand(tenantId, fieldId, request.Label, request.SortOrder),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.CustomFields);
    }

    internal sealed class Request
    {
        public string Label { get; init; } = string.Empty;

        public int SortOrder { get; init; }
    }
}
