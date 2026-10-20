using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest(IntegrationTestWebAppFactory factory) : IDisposable
{
    protected HttpClient HttpClient { get; } = factory.CreateClient();

    protected TestTimeProvider Clock => factory.Clock;

    public async Task<Result<TResult>> SendCommand<TCommand, TResult>(TCommand command)
        where TCommand : ICommand<TResult>
    {
        using IServiceScope scope = factory.Services.CreateScope();

        ICommandHandler<TCommand, TResult> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();

        return await handler.Handle(command, CancellationToken.None);
    }

    public async Task<Result<TResult>> SendQuery<TQuery, TResult>(TQuery query)
        where TQuery : IQuery<TResult>
    {
        using IServiceScope scope = factory.Services.CreateScope();

        IQueryHandler<TQuery, TResult> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        return await handler.Handle(query, CancellationToken.None);
    }

    protected IServiceScope CreateScope() => factory.Services.CreateScope();

    protected async Task ExecuteSqlAsync(FormattableString sql)
    {
        using IServiceScope scope = factory.Services.CreateScope();

        CustomersDbContext dbContext = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();

        await dbContext.Database.ExecuteSqlAsync(sql);
    }

    protected async Task<T> QuerySingleAsync<T>(FormattableString sql)
    {
        using IServiceScope scope = factory.Services.CreateScope();

        CustomersDbContext dbContext = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();

        return await dbContext.Database.SqlQuery<T>(sql).SingleAsync();
    }

    public void Dispose()
    {
        HttpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
