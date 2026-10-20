using AwesomeAssertions;
using FluentValidation;
using SubscriptionCommerce.Common.Application.Behaviors;
using SubscriptionCommerce.Common.Application.Messaging;
using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Common.UnitTests.Behaviors;

public class ValidationDecoratorTests
{
    [Fact]
    public async Task Handle_Should_RunValidatorsOneAfterAnother_AndReportEveryFailureInOrder()
    {
        var running = new RunningCount();
        var decorator = new ValidationDecorator.QueryHandler<TestQuery, int>(
            new AnswerHandler(),
            [new SlowFailingValidator("First", running), new SlowFailingValidator("Second", running)]);

        Result<int> result = await decorator.Handle(new TestQuery(), CancellationToken.None);

        running.Max.Should().Be(1);
        result.Error.Should().BeOfType<ValidationError>()
            .Which.Errors.Select(e => e.Code).Should().Equal("First", "Second");
    }

    [Fact]
    public async Task Handle_Should_CallTheHandler_WhenThereAreNoValidators()
    {
        var decorator = new ValidationDecorator.QueryHandler<TestQuery, int>(new AnswerHandler(), []);

        Result<int> result = await decorator.Handle(new TestQuery(), CancellationToken.None);

        result.Value.Should().Be(42);
    }

    private sealed record TestQuery : IQuery<int>;

    private sealed class AnswerHandler : IQueryHandler<TestQuery, int>
    {
        public Task<Result<int>> Handle(TestQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success(42));
    }

    private sealed class RunningCount
    {
        private int _current;

        public int Max { get; private set; }

        public void Enter() => Max = Math.Max(Max, Interlocked.Increment(ref _current));

        public void Leave() => Interlocked.Decrement(ref _current);
    }

    private sealed class SlowFailingValidator : AbstractValidator<TestQuery>
    {
        public SlowFailingValidator(string code, RunningCount running) =>
            RuleFor(q => q)
                .MustAsync(async (_, cancellationToken) =>
                {
                    running.Enter();
                    await Task.Delay(50, cancellationToken);
                    running.Leave();
                    return false;
                })
                .WithErrorCode(code);
    }
}
