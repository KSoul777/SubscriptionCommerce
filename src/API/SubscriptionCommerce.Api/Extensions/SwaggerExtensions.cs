using Microsoft.OpenApi;

namespace SubscriptionCommerce.Api.Extensions;

internal static class SwaggerExtensions
{
    internal static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Subscription Commerce API",
                Version = "v1",
                Description = "Discount calculation (Pricing) and dynamic customer fields (Customers)."
            });

            // Every endpoint has a nested Request class; full names keep the schema ids unique.
            options.CustomSchemaIds(t => t.FullName?.Replace("+", ".", StringComparison.Ordinal));
        });

        return services;
    }
}
