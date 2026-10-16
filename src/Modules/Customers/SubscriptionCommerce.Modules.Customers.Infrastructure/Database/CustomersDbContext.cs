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
            throw new ConcurrencyConflictException("A row was changed by another request", exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // 2601/2627: unique index or unique constraint violation.
            throw new ConcurrencyConflictException("A unique key was taken by another request", exception);
        }
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

    // A change can touch only the selected options (a child table) and leave the value row's own columns
    // as they were, for example two changes in the same clock tick. Marking the row modified makes EF update
    // it anyway, so every change runs the rowversion check and moves the row version on.
    private void MarkValuesWithNewHistoryAsModified()
    {
        IEnumerable<EntityEntry<CustomerFieldValue>> touchedValues = ChangeTracker
            .Entries<CustomerFieldValue>()
            .Where(e => e.State == EntityState.Unchanged && e.Entity.History.Any(h => h.Id == 0));

        foreach (EntityEntry<CustomerFieldValue> entry in touchedValues)
        {
            entry.Property(v => v.UpdatedAtUtc).IsModified = true;
        }
    }
}
