using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Customers;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Fields;
using SubscriptionCommerce.Modules.Customers.Infrastructure.FieldValues;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Tenants;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

public sealed class CustomersDbContext(DbContextOptions<CustomersDbContext> options) : DbContext(options), IUnitOfWork
{
    private static readonly Dictionary<string, ConflictKind> UniqueIndexes = new(StringComparer.Ordinal)
    {
        [CustomFieldDefinitionConfiguration.UniqueNameIndex] = ConflictKind.DuplicateFieldName,
        [CustomFieldOptionConfiguration.UniqueLabelIndex] = ConflictKind.DuplicateOptionLabel,
        [CustomerFieldValueConfiguration.OneValuePerFieldIndex] = ConflictKind.DuplicateFieldValue,
        [CustomerFieldValueOptionConfiguration.PrimaryKey] = ConflictKind.DuplicateSelectedOption
    };

    internal DbSet<Tenant> Tenants => Set<Tenant>();

    internal DbSet<Customer> Customers => Set<Customer>();

    internal DbSet<CustomFieldDefinition> FieldDefinitions => Set<CustomFieldDefinition>();

    internal DbSet<CustomerFieldValue> FieldValues => Set<CustomerFieldValue>();

    internal DbSet<CustomerFieldValueHistory> FieldValueHistory => Set<CustomerFieldValueHistory>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        MarkValuesWithNewHistoryAsModified();

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(ConflictKind.RowVersion, "A row was changed by another request", exception);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 } sqlException &&
            UniqueKeyConflict(sqlException.Message) is { } kind)
        {
            throw new ConcurrencyConflictException(kind, "A unique key was taken by another request", exception);
        }
    }

    internal static ConflictKind? UniqueKeyConflict(string sqlErrorMessage)
    {
        foreach ((string index, ConflictKind kind) in UniqueIndexes)
        {
            if (sqlErrorMessage.Contains($"'{index}'", StringComparison.Ordinal))
            {
                return kind;
            }
        }

        return null;
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Customers);

        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomFieldDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new CustomFieldOptionConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerFieldValueConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerFieldValueOptionConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerFieldValueHistoryConfiguration());
    }

    private void MarkValuesWithNewHistoryAsModified()
    {
        IEnumerable<EntityEntry<CustomerFieldValue>> touchedValues = ChangeTracker
            .Entries<CustomerFieldValue>()
            .Where(e => e.State == EntityState.Unchanged && e.Entity.PendingHistory.Any(h => h.Id == 0));

        foreach (EntityEntry<CustomerFieldValue> entry in touchedValues)
        {
            entry.Property(v => v.UpdatedAtUtc).IsModified = true;
        }
    }
}
