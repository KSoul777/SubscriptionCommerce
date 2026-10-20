using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SubscriptionCommerce.Common.Application.Behaviors;
using SubscriptionCommerce.Common.Application.Exceptions;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Common.UnitTests.Behaviors;

public class ExceptionHandlingDecoratorTests
{
    public static TheoryData<string> Variants => ["Command", "CommandWithResponse", "Query"];

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task Handle_Should_LetTheCancellationThrough_WhenTheCallerCancelled(string variant)
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var cancelled = new OperationCanceledException(cancellation.Token);

        Func<Task> handle = () => HandleThrough(variant, cancelled, cancellation.Token);

        (await handle.Should().ThrowExactlyAsync<OperationCanceledException>()).Which.Should().BeSameAs(cancelled);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task Handle_Should_WrapACancellation_TheCallerDidNotAskFor(string variant)
    {
        var timeout = new TaskCanceledException("The database call timed out");

        Func<Task> handle = () => HandleThrough(variant, timeout, CancellationToken.None);

        SubscriptionCommerceException wrapped = (await handle.Should().ThrowExactlyAsync<SubscriptionCommerceException>()).Which;

        wrapped.InnerException.Should().BeSameAs(timeout);
        wrapped.RequestName.Should().Be($"Test{variant}");
        wrapped.Message.Should().Be($"An unexpected exception occurred while handling Test{variant}.");
    }

    private static Task HandleThrough(string variant, Exception exception, CancellationToken cancellationToken)
    {
        var inner = new ThrowingHandler(exception);

        return variant switch
        {
            "Command" => new ExceptionHandlingDecorator.CommandHandler<TestCommand>(
                    inner, NullLogger<ExceptionHandlingDecorator.CommandHandler<TestCommand>>.Instance)
                .Handle(new TestCommand(), cancellationToken),
            "CommandWithResponse" => new ExceptionHandlingDecorator.CommandHandlerT<TestCommandWithResponse, int>(
                    inner, NullLogger<ExceptionHandlingDecorator.CommandHandlerT<TestCommandWithResponse, int>>.Instance)
                .Handle(new TestCommandWithResponse(), cancellationToken),
            "Query" => new ExceptionHandlingDecorator.QueryHandler<TestQuery, int>(
                    inner, NullLogger<ExceptionHandlingDecorator.QueryHandler<TestQuery, int>>.Instance)
                .Handle(new TestQuery(), cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null)
        };
    }

    private sealed record TestCommand : ICommand;

    private sealed record TestCommandWithResponse : ICommand<int>;

    private sealed record TestQuery : IQuery<int>;

    private sealed class ThrowingHandler(Exception exception)
        : ICommandHandler<TestCommand>,
          ICommandHandler<TestCommandWithResponse, int>,
          IQueryHandler<TestQuery, int>
    {
        public Task<Result> Handle(TestCommand command, CancellationToken cancellationToken) =>
            Task.FromException<Result>(exception);

        public Task<Result<int>> Handle(TestCommandWithResponse command, CancellationToken cancellationToken) =>
            Task.FromException<Result<int>>(exception);

        public Task<Result<int>> Handle(TestQuery query, CancellationToken cancellationToken) =>
            Task.FromException<Result<int>>(exception);
    }
}
