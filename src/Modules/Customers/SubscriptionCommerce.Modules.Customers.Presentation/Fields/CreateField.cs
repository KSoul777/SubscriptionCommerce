using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.Fields.CreateField;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.Presentation.Fields;

internal sealed class CreateField : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenants/{tenantId:int}/fields", async (
            int tenantId,
            Request request,
            ICommandHandler<CreateFieldCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            Result<int> result = await handler.Handle(
                new CreateFieldCommand(
                    tenantId,
                    request.Name,
                    request.Type,
                    request.AllowMultipleSelection,
                    request.IsRequired,
                    request.SortOrder),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.CustomFields);
    }

    internal sealed class Request
    {
        public string Name { get; init; } = string.Empty;

        public FieldType Type { get; init; }

        public bool AllowMultipleSelection { get; init; }

        public bool IsRequired { get; init; }

        public int SortOrder { get; init; }
    }
}
