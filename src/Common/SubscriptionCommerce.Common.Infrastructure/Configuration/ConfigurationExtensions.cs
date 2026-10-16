using Microsoft.Extensions.Configuration;

namespace SubscriptionCommerce.Common.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static string GetConnectionStringOrThrow(this IConfiguration configuration, string name)
    {
        string? connectionString = configuration.GetConnectionString(name);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"The connection string {name} was not found");
        }

        return connectionString;
    }
}
