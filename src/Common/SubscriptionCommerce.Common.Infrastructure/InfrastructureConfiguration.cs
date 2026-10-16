using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SubscriptionCommerce.Common.Application.Authentication;
using SubscriptionCommerce.Common.Infrastructure.Authentication;

namespace SubscriptionCommerce.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string serviceName)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUser, HeaderCurrentUser>();

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                tracing.AddOtlpExporter();
            });

        return services;
    }
}
