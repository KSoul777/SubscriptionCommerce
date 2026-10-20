using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

internal sealed class UtcDateTimeConverter()
    : ValueConverter<DateTime, DateTime>(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
