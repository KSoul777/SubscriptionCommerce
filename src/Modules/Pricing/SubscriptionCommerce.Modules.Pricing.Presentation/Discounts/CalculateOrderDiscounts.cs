using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Common.Presentation.Results;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts.CalculateOrderDiscounts;

namespace SubscriptionCommerce.Modules.Pricing.Presentation.Discounts;

internal sealed class CalculateOrderDiscounts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders/discounts", async (
            Request request,
            IQueryHandler<CalculateOrderDiscountsQuery, DiscountBreakdownResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<DiscountBreakdownResponse> result = await handler.Handle(
                new CalculateOrderDiscountsQuery(
                    request.CustomerId,
                    request.Price,
                    request.Currency,
                    request.CouponCodes ?? []),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Discounts);
    }

    internal sealed class Request
    {
        public int CustomerId { get; init; }

        public decimal Price { get; init; }

        public string Currency { get; init; } = string.Empty;

        public IReadOnlyCollection<string>? CouponCodes { get; init; }
    }
}
