using Microsoft.AspNetCore.Routing;

namespace SubscriptionCommerce.Common.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
