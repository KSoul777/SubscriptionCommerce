using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;

namespace SubscriptionCommerce.Modules.Customers.Presentation.FieldValues;

internal sealed class SetCustomerFieldValue : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("customers/{customerId:int}/fields/{fieldId:int}/value", async (
            int customerId,
            int fieldId,
            Request request,
            ICommandHandler<SetCustomerFieldValueCommand, SetCustomerFieldValueResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<SetCustomerFieldValueResponse> result = await handler.Handle(
                new SetCustomerFieldValueCommand(
                    customerId,
                    fieldId,
                    request.TextValue,
                    request.SelectedOptionIds ?? [],
                    request.RowVersion),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Customers);
    }

    internal sealed class Request
    {
        public string? TextValue { get; init; }

        public IReadOnlyCollection<int>? SelectedOptionIds { get; init; }

        public string? RowVersion { get; init; }
    }
}
