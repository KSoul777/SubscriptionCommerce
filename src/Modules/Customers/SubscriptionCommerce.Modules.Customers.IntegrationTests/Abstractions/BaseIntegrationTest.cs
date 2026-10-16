using Microsoft.Extensions.DependencyInjection;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

// Tests share one database and stay independent by creating their own tenant, customer and fields.
[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest(IntegrationTestWebAppFactory factory) : IDisposable
{
    protected HttpClient HttpClient { get; } = factory.CreateClient();

    protected TestTimeProvider Clock => factory.Clock;

    // Each call gets its own DI scope, and so its own DbContext, like a separate HTTP request.
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

    public void Dispose()
    {
        HttpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
