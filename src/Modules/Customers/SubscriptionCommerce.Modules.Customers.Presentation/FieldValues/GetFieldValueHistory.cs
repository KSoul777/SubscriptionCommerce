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
        app.MapGet("tenants/{tenantId:int}/customers/{customerId:int}/fields/{fieldId:int}/history", async (
            int tenantId,
            int customerId,
            int fieldId,
            long? after,
            int? limit,
            IQueryHandler<GetFieldValueHistoryQuery, FieldValueHistoryPage> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFieldValueHistoryQuery(
                tenantId, customerId, fieldId, after, limit ?? GetFieldValueHistoryQuery.DefaultLimit);

            Result<FieldValueHistoryPage> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Customers);
    }
}
