using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

// All timestamps are written in UTC. SQL Server's datetime2 has no kind, so they are marked UTC again on read.
// Without this the API would return them without the trailing Z.
internal sealed class UtcDateTimeConverter()
    : ValueConverter<DateTime, DateTime>(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
