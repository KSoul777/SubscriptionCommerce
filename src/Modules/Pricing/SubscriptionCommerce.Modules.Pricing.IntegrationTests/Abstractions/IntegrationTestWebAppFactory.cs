using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace SubscriptionCommerce.Modules.Pricing.IntegrationTests.Abstractions;

// Runs the real API against a throw-away SQL Server. The Development environment applies the
// generated migrations on start, including the seeded example data from the brief.
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // The container's connection string points at master; use a database of our own.
        string connectionString = new SqlConnectionStringBuilder(_dbContainer.GetConnectionString())
        {
            InitialCatalog = "SubscriptionCommerce"
        }.ConnectionString;

        Environment.SetEnvironmentVariable("ConnectionStrings__Database", connectionString);

        builder.UseEnvironment("Development");
    }
}
