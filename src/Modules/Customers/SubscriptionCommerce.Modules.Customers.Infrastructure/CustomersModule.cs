using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionCommerce.Common.Infrastructure.Configuration;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Customers;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Fields;
using SubscriptionCommerce.Modules.Customers.Infrastructure.FieldValues;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Tenants;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure;

public static class CustomersModule
{
    public static IServiceCollection AddCustomersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CustomersDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionStringOrThrow("Database"),
                sqlOptions => sqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Customers)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CustomersDbContext>());

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomFieldDefinitionRepository, CustomFieldDefinitionRepository>();
        services.AddScoped<ICustomerFieldValueRepository, CustomerFieldValueRepository>();
    }
}
