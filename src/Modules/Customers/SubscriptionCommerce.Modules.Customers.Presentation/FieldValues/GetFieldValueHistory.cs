using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;

namespace SubscriptionCommerce.Modules.Customers.Presentation.FieldValues;

internal sealed class GetFieldValueHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("customers/{customerId:int}/fields/{fieldId:int}/history", async (
            int customerId,
            int fieldId,
            IQueryHandler<GetFieldValueHistoryQuery, IReadOnlyList<FieldValueHistoryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<IReadOnlyList<FieldValueHistoryResponse>> result =
                await handler.Handle(new GetFieldValueHistoryQuery(customerId, fieldId), cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Customers);
    }
}
